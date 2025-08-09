// Shader Graph Baker (Free) <https://u3d.as/3ycp>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEditor.ShaderGraph;


namespace AmazingAssets.ShaderGraphBakerFree.Editor
{
    static internal class TextureBaker
    {
        static Mesh quadMesh = Resources.GetBuiltinResource(typeof(Mesh), "Quad.fbx") as Mesh;
        static int minTextureSize = 16;
        static string shaderTimeParameter = "_TimeParameters";

        static internal void BakeTexture(Node node)
        {
            string saveDirectory;
            string saveFileName;
            string saveExtension;
            bool savePathIsProjectRelative;
            if (GetTextureSavePathOptions(node, out saveDirectory, out saveFileName, out saveExtension, out savePathIsProjectRelative) == false)
                return;


            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            Material material = GetPreviewMaterial(node, materialPropertyBlock);
            if (material == null)
                return;


            int textureSize;
            GetTextureSize(node, out textureSize);


            //Strange bug in Unity 6000.0.x URP, quadMesh is not loaded using - Resources.GetBuiltinResource(typeof(Mesh), "Quad.fbx")
            GameObject quadMeshObject = null;
#if UNITY_6000_0
            if(EditorUtilities.GetCurrentRenderPipeline() == EditorUtilities.RenderPipeline.Universal)
                quadMeshObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
#endif



            string lastSavedFilePath = string.Empty;

            Texture2D texture = GetPreviewTexture(material, materialPropertyBlock, textureSize, textureSize, node.SuperSize, false);
            if (texture != null)
            {
                string savePath = Path.Combine(saveDirectory, $"{saveFileName}.{saveExtension}");

                SaveTexture(texture, node.Format, savePath);
                EditorUtilities.DestroyUnityObject(texture);

                AssetDatabase.Refresh();

                lastSavedFilePath = savePath;
            }


            //Highlight last saved file inside Project window
            if (savePathIsProjectRelative)
            {
                UnityEngine.Object lastSavedFile = AssetDatabase.LoadAssetAtPath(lastSavedFilePath, typeof(Texture));

                if (node.OutputTexture == null)
                    UnityEditor.EditorGUIUtility.PingObject(lastSavedFile);
                else
                    node.OutputTexture = (Texture)lastSavedFile;
            }


            //Cleanup           
            EditorUtilities.DestroyUnityObject(material.shader);
            EditorUtilities.DestroyUnityObject(material);
            EditorUtilities.DestroyUnityObject(quadMeshObject);

            Resources.UnloadUnusedAssets();
        }
        static bool GetTextureSavePathOptions(Node node, out string saveDirectory, out string saveFileName, out string saveExtension, out bool savePathIsProjectRelative)
        {
            saveDirectory = string.Empty;
            saveFileName = string.Empty;
            saveExtension = string.Empty;
            savePathIsProjectRelative = false;


            if (node.OutputTexture != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(node.OutputTexture);

                if (string.IsNullOrWhiteSpace(assetPath) == false)
                {
                    if (Path.GetExtension(assetPath).ToLowerInvariant() == ("." + GetTextureSaveExtension(node.Format)))
                    {
                        saveDirectory = Path.GetDirectoryName(assetPath);
                        saveFileName = Path.GetFileNameWithoutExtension(assetPath);
                        saveExtension = Path.GetExtension(assetPath).ToLowerInvariant().Replace(".", string.Empty);

                        return true;
                    }
                    else
                    {
                        string newPath = Path.Combine(Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath)) + "." + GetTextureSaveExtension(node.Format);

                        //Change file extension
                        try
                        {
                            File.Move(assetPath, newPath);
                        }
                        catch (System.Exception)
                        {
                            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(newPath, typeof(Texture));
                            ShaderGraphBakerDebug.Log(LogType.Error, $"Cannot overwrite selected texture. File with the same name and extension already exists:\n'{newPath}'.\n", null, asset);

                            return false;
                        }


                        //Update meta
                        if (File.Exists(assetPath + ".meta"))
                            File.Move(assetPath + ".meta", newPath + ".meta");

                        AssetDatabase.Refresh();


                        saveDirectory = Path.GetDirectoryName(newPath);
                        saveFileName = Path.GetFileNameWithoutExtension(newPath);
                        saveExtension = Path.GetExtension(newPath).ToLowerInvariant().Replace(".", string.Empty);

                        return true;
                    }
                }
            }


            string savePanelPath;
            //Get current ShaderGraph file path
            string graphPath = string.Empty;
            GUID guid;
            if (GUID.TryParse(node.owner.owner.graph.assetGuid, out guid))
                graphPath = AssetDatabase.GUIDToAssetPath(guid);


            if (File.Exists(graphPath))
                savePanelPath = UnityEditor.EditorUtility.SaveFilePanel("Save Texture", Path.GetDirectoryName(graphPath), Path.GetFileNameWithoutExtension(graphPath), GetTextureSaveExtension(node.Format));
            else
                savePanelPath = UnityEditor.EditorUtility.SaveFilePanel("Save Texture", "Assets", "New Shader Graph Texture", GetTextureSaveExtension(node.Format));


            if (string.IsNullOrWhiteSpace(savePanelPath) == false)
            {
                //If extension was changed inside the SavePanel to something unsupported
                string savePanelExtension = Path.GetExtension(savePanelPath).ToLowerInvariant();
                if ((savePanelExtension == ".jpg" || savePanelExtension == ".png" || savePanelExtension == ".tga") == false)
                    savePanelPath = Path.ChangeExtension(savePanelPath, "." + GetTextureSaveExtension(node.Format));


                //Check if path is project relative
                savePathIsProjectRelative = EditorUtilities.IsPathProjectRelative(savePanelPath);


                //Adjust path
                if (savePathIsProjectRelative)
                    savePanelPath = EditorUtilities.ConvertPathToProjectRelative(savePanelPath);


                saveDirectory = Path.GetDirectoryName(savePanelPath);
                saveFileName = Path.GetFileNameWithoutExtension(savePanelPath);
                saveExtension = Path.GetExtension(savePanelPath).ToLowerInvariant().Replace(".", string.Empty);

                node.lastSavedTextureFilePath = savePanelPath;


                return true;
            }


            return false;
        }
        static void SaveTexture(Texture2D texture, Enum.Format format, string savePath)
        {
            byte[] bytes;
            switch (format)
            {
                case Enum.Format.JPG: bytes = texture.EncodeToJPG(100); break;
                case Enum.Format.TGA: bytes = texture.EncodeToTGA(); break;

                case Enum.Format.PNG:
                default: bytes = texture.EncodeToPNG(); break;
            }

            File.WriteAllBytes(savePath, bytes);
        }
        static Texture2D GetPreviewTexture(Material material, MaterialPropertyBlock materialPropertyBlock, int width, int height, Enum.SuperSize superSize, bool mipChain)
        {
            //Texture resolution
            int superWidth, superHeight;
            GetSuperSizeWidthAndHeight(width, height, superSize, out superWidth, out superHeight);



            //Texture formats
            TextureFormat textureFormat;
            RenderTextureFormat renderTextureFormat;
            GetTextureFormats(out textureFormat, out renderTextureFormat);

                     
            //Create textures
            RenderTexture renderTexture = RenderTexture.GetTemporary(superWidth, superHeight, 16, renderTextureFormat, RenderTextureReadWrite.Default);
            RenderTexture renderTextureSuperSize = superSize == Enum.SuperSize.None ? null : RenderTexture.GetTemporary(width, height, 16, renderTextureFormat, RenderTextureReadWrite.Default);
            Texture2D texture = new Texture2D(width, height, textureFormat, mipChain);


            RenderTexture.active = null;

            //Setup render camera
            GameObject cameraGO = new GameObject();
            cameraGO.transform.position = Vector3.forward * -1;
            cameraGO.transform.rotation = Quaternion.identity;

            Camera camera = cameraGO.AddComponent<Camera>();
            camera.enabled = false;
            camera.cameraType = CameraType.Preview;
            camera.orthographic = true;
            camera.orthographicSize = 0.5f;
            camera.farClipPlane = 10.0f;
            camera.nearClipPlane = 1.0f;
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.clear;
            camera.renderingPath = RenderingPath.Forward;
            camera.useOcclusionCulling = false;
            camera.allowMSAA = false;
            camera.allowHDR = true;


            camera.targetTexture = renderTexture;
            Graphics.DrawMesh(quadMesh, Matrix4x4.identity, material, 1, camera, 0, materialPropertyBlock, ShadowCastingMode.Off, false, null, false);
            camera.Render();


            if (superSize == Enum.SuperSize.None)
            {
                RenderTexture.active = renderTexture;
            }
            else
            {
                Graphics.Blit(renderTexture, renderTextureSuperSize);

                RenderTexture.active = renderTextureSuperSize;
            }

            
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0, mipChain);
            texture.Apply(mipChain);



            //Cleanup
            RenderTexture.active = null;

            EditorUtilities.DestroyUnityObject(cameraGO);
            RenderTexture.ReleaseTemporary(renderTexture);
            if (renderTextureSuperSize != null)
                RenderTexture.ReleaseTemporary(renderTextureSuperSize);



            return texture;
        }
        static internal string GetPreviewShader(Node node)
        {
            var wasAsyncAllowed = ShaderUtil.allowAsyncCompilation;
            ShaderUtil.allowAsyncCompilation = true;

            Generator generator = new Generator(node.owner, node, GenerationMode.ForReals, $"hidden/preview/{node.GetVariableNameForNode()}", null);
            string generatedShader = generator.generatedShader;

            if (node.FindInputSlot<MaterialSlot>(Node.InputSlotId).concreteValueType == ConcreteSlotValueType.Vector4)
            {
                ShaderStringBuilder shaderStringBuilder = new ShaderStringBuilder();
                node.GenerateNodeCode(shaderStringBuilder, GenerationMode.Preview);
                string sData = shaderStringBuilder.ToString();
                sData = sData.Substring(0, sData.IndexOf(';'));
                if (sData.Contains("$precision4 "))
                {
                    string varName = sData.Substring(sData.IndexOf(" ") + 1);
                    generatedShader = generatedShader.Replace($"{varName}.z, 1.0", $"{varName}.z, 1");
                }
            }

            ShaderUtil.allowAsyncCompilation = wasAsyncAllowed;

            return generatedShader;
        }
        static Material GetPreviewMaterial(Node node, MaterialPropertyBlock materialPropertyBlock)
        {
            HashSet<AbstractMaterialNode> sources = new HashSet<AbstractMaterialNode>() { node };
            HashSet<AbstractMaterialNode> nodesToDraw = new HashSet<AbstractMaterialNode>();
            PreviewManager.PropagateNodes(sources, PreviewManager.PropagationDirection.Upstream, nodesToDraw);

            PooledList<PreviewProperty> perMaterialPreviewProperties = PooledList<PreviewProperty>.Get();
            PreviewManager.CollectPreviewProperties(node.owner, nodesToDraw, perMaterialPreviewProperties, materialPropertyBlock);


            string shaderString = GetPreviewShader(node);


            //Check if 'TimeParameter' is used
            if (shaderString.Contains(shaderTimeParameter))
            {
                ShaderGraphBakerDebug.Log(LogType.Warning, "ShaderGraph uses 'Time' node. Baked texture may be incorrect.\n");
            }



            Material material = null;
            Shader shader = ShaderUtil.CreateShaderAsset(shaderString);
            if (shader != null && ShaderUtil.ShaderHasError(shader) == false)
            {
                material = new Material(shader);
                PreviewManager.AssignPerMaterialPreviewProperties(material, perMaterialPreviewProperties);
            }
            else
            {
                ShaderGraphBakerDebug.Log(LogType.Error, "Cannot create shader.\n");
            }

            

            return material;
        }

        static string GetTextureSaveExtension(Enum.Format format)
        {
            return format.ToString().ToLowerInvariant();
        }
        static void GetTextureSize(Node node, out int textureSize)
        {
            if (node.Resolution == Enum.Resolution.Custom)
            {
                textureSize = EditorUtilities.TryParseInt(node.GetSlotValue(Node.CustomResolutionSlotID, GenerationMode.ForReals), minTextureSize);
            }
            else
            {
                textureSize = EditorUtilities.TryParseInt(node.Resolution.ToString(), minTextureSize);
            }

            textureSize = (int)Mathf.Clamp(textureSize, minTextureSize, SystemInfo.maxTextureSize);
        }
        static void GetSuperSizeWidthAndHeight(int width, int height, Enum.SuperSize superSize, out int superWidth, out int superHeight)
        {
            if (superSize != Enum.SuperSize.None)
            {
                int scale = superSize == Enum.SuperSize._4 ? 4 : 2;


                if (width == height)
                {
                    if (width * scale < SystemInfo.maxTextureSize)
                        width *= scale;

                    height = width;
                }
                else if (width > height)
                {
                    if (width * scale < SystemInfo.maxTextureSize)
                    {
                        width *= scale;
                        height *= scale;
                    }
                }
                else
                {
                    if (height * scale < SystemInfo.maxTextureSize)
                    {
                        width *= scale;
                        height *= scale;
                    }
                }
            }

            superWidth = width;
            superHeight = height;
        }

        static void GetTextureFormats(out TextureFormat textureFormat, out RenderTextureFormat renderTextureFormat)
        {
            renderTextureFormat = RenderTextureFormat.ARGB32;

            textureFormat = TextureFormat.ARGB32;
        }
    }
}
