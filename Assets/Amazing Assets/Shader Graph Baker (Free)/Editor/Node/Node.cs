// Shader Graph Baker (Free) <https://u3d.as/3ycp>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using System.Collections.Generic;

using UnityEngine;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Internal;
using UnityEditor.ShaderGraph.Drawing.Controls;


namespace AmazingAssets.ShaderGraphBakerFree.Editor
{
    internal class Enum
    {
        public enum Mode { Single, Sequence, Atlas, [InspectorName("2D Array")] _2DArray, [InspectorName("Texture 3D")] Texture3D }
        public enum Type {[InspectorName("Color Map")] ColorMap, Normal, [InspectorName("Raw Data")] RawData }        
        public enum Format { JPG, PNG, TGA, EXR, EXRZip}
        public enum Resolution 
        {
            [InspectorName("16")] _16,
            [InspectorName("32")] _32,
            [InspectorName("64")] _64,
            [InspectorName("128")] _128,
            [InspectorName("256")] _256,
            [InspectorName("512")] _512,
            [InspectorName("1024")] _1024,
            [InspectorName("2048")] _2048,
            [InspectorName("4096")] _4096,
            [InspectorName("8192")] _8192,

            Custom 
        }
        public enum SuperSize 
        { 
            None,

            [InspectorName("2")] _2,
            [InspectorName("4")] _4 
        }
    }

    [Title("Amazing Assets", "Shader Graph Baker (Free)")]
    class Node : AbstractMaterialNode, IGeneratesBodyCode, IGeneratesFunction
    {
        public override string documentationURL => ShaderGraphBakerAbout.documentationURL;

         
        public override bool hasPreview { get { return true; } }


        public const int InputSlotId = 0;          
        public const int CustomResolutionSlotID = 6;
        const int OutputSlotId = 7;
        const string kInputSlotName = "Input";      
        const string kCustomResolutionSlotName = "Custom Resolution";
        const string kOutputSlotName = "Out";
                   
        [SerializeField] float m_CustomResolution = 100;

        static bool isBaking = false;
        public string lastSavedTextureFilePath = string.Empty;
        public string lastSavedShaderFilePath = string.Empty;


        [SerializeField]
        private Enum.Mode m_Mode = Enum.Mode.Single;
        [EnumControl("Mode")]
        public Enum.Mode Mode
        {
            get { return m_Mode; }
            set 
            {
                if (m_Mode == value)
                    return;

                m_Mode = value;

                UpdateTextureBakerButtonName();
                UpdateNodeAfterDeserialization();
                Dirty(ModificationScope.Node);
            }
        } 
            
        [SerializeField]
        private Enum.Type m_TextureType = Enum.Type.ColorMap;
        [EnumControl("Type")]
        public Enum.Type TextureType
        {
            get { return m_TextureType; }
            set
            {
                if (m_TextureType == value)
                    return;

                m_TextureType = value;

                UpdateTextureBakerButtonName();
                UpdateShaderBakerButtonName();

                Dirty(ModificationScope.Topological);
            }
        }
               
        [SerializeField]
        private Enum.Resolution m_Resolution = Enum.Resolution._1024;
        [EnumControl("Resolution")]
        public Enum.Resolution Resolution
        {
            get { return m_Resolution; }
            set
            {
                if (m_Resolution == value)
                    return;

                m_Resolution = value;

                UpdateNodeAfterDeserialization();
                Dirty(ModificationScope.Node);
            }
        }

        [SerializeField]
        private Enum.SuperSize m_SuperSize = Enum.SuperSize.None;
        [EnumControl("Super Size")]
        public Enum.SuperSize SuperSize
        {
            get { return m_SuperSize; }
            set
            {
                if (m_SuperSize == value)
                    return;

                m_SuperSize = value;

                Dirty(ModificationScope.Node);
            }
        }

        [SerializeField]
        private Enum.Format m_Format = Enum.Format.PNG;
        [EnumControl("Format")]
        public Enum.Format Format
        {
            get { return m_Format; }
            set
            {
                if (m_Format == value)
                    return;

                m_Format = value;

                m_OutputTexture.texture = null;

                UpdateTextureBakerButtonName();
                UpdateShaderBakerButtonName();

                Dirty(ModificationScope.Node);
            }
        }

        [ButtonControl("InitTextureBakerButton", "TextureBakerButtonCallback")]
        int textureBakerButtonControll { get; set; }
        public UnityEngine.UIElements.Button m_TextureBakerButton;
        
        [SerializeField]
        private SerializableTexture m_OutputTexture = new SerializableTexture();
        [TextureControl("")]
        public Texture OutputTexture
        {
            get
            { 
                return m_OutputTexture.texture;
            }

            set
            {
                m_OutputTexture.texture = value;

                UpdateTextureBakerButtonName();
                Dirty(ModificationScope.Node);
            }
        }

        [ButtonControl("InitShaderBakerButton", "ShaderBakerButtonCallback")]
        int shaderBakerButtonControll { get; set; }
        public UnityEngine.UIElements.Button m_ShaderBakerButton;



        public Node()
        {
            name = "Shader Graph Baker (Free)";

            m_PreviewMode = PreviewMode.Preview2D;

            UpdateNodeAfterDeserialization();
        }
        public override void ValidateNode()
        {
            base.ValidateNode();

            UpdateTextureBakerButtonName();
            UpdateShaderBakerButtonName();
        }
        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new DynamicVectorMaterialSlot(InputSlotId, kInputSlotName, kInputSlotName, SlotType.Input, Vector3.zero));
            AddSlot(new DynamicVectorMaterialSlot(OutputSlotId, kOutputSlotName, kOutputSlotName, SlotType.Output, Vector3.zero, ShaderStageCapability.All, true));

            List<int> slotIDs = new List<int> { InputSlotId, OutputSlotId };


            RemoveSlotsNameNotMatching(slotIDs.ToArray(), true);


            if (Resolution == Enum.Resolution.Custom)
            {
                AddSlot(new Vector1MaterialSlot(CustomResolutionSlotID, kCustomResolutionSlotName, kCustomResolutionSlotName, SlotType.Input, m_CustomResolution, ShaderStageCapability.All, string.Empty));

                slotIDs.Add(CustomResolutionSlotID);
            }


            RemoveSlotsNameNotMatching(slotIDs.ToArray(), true);
        }
        string GetFunctionName()
        {
            return $"ShaderGraphBaker_{FindSlot<MaterialSlot>(InputSlotId).concreteValueType.ToShaderString()}";
        }
        public void GenerateNodeCode(ShaderStringBuilder sb, GenerationMode generationMode)
        {
            var inputValue = GetSlotValue(InputSlotId, generationMode);
            var outputValue = GetVariableNameForSlot(OutputSlotId);
            sb.AppendLine("{0} {1};", FindOutputSlot<MaterialSlot>(OutputSlotId).concreteValueType.ToShaderString(), outputValue);

            sb.AppendLine("{0}({1}, {2});", GetFunctionName(), inputValue, outputValue);
        }
        public void GenerateNodeFunction(FunctionRegistry registry, GenerationMode generationMode)
        {
            registry.ProvideFunction(GetFunctionName(), s =>
            {
                s.AppendLine("void {0}({1} In, out {2} Out)",
                    GetFunctionName(),
                    FindInputSlot<MaterialSlot>(InputSlotId).concreteValueType.ToShaderString(),
                    FindOutputSlot<MaterialSlot>(OutputSlotId).concreteValueType.ToShaderString());
                using (s.BlockScope())
                {
                    s.AppendLine(GetNodeFunctionBody());
                }
            });
        }           
        public override void CollectPreviewMaterialProperties(List<PreviewProperty> properties)
        {
            base.CollectPreviewMaterialProperties(properties);

            if (Resolution == Enum.Resolution.Custom)
            {
                m_CustomResolution = EditorUtilities.TryParseFloat(GetSlotValue(CustomResolutionSlotID, GenerationMode.ForReals), 1024);
            }
        }
        string GetNodeFunctionBody()
        {
            return "{Out = In;}";            
        }
         
        public void InitTextureBakerButton(UnityEngine.UIElements.Button button)
        {
            m_TextureBakerButton = button;

            UpdateTextureBakerButtonName();
        }
        public void UpdateTextureBakerButtonName()
        {
            if (m_TextureBakerButton != null)
            {
                if (TextureType == Enum.Type.ColorMap && Mode == Enum.Mode.Single && (Format == Enum.Format.JPG || Format == Enum.Format.PNG || Format == Enum.Format.TGA))
                {
                    m_TextureBakerButton.SetEnabled(true);
                    m_TextureBakerButton.text = (m_OutputTexture.texture == null ? "Bake" : "Overwrite") + " (Color Map)";
                }
                else
                {
                    m_TextureBakerButton.SetEnabled(false);
                    m_TextureBakerButton.text = "Not available in the free version";
                }
            }
        }
        public void TextureBakerButtonCallback()
        {
            if (TextureType == Enum.Type.ColorMap && Mode == Enum.Mode.Single && (Format == Enum.Format.JPG || Format == Enum.Format.PNG || Format == Enum.Format.TGA))
            {
                isBaking = true;
                {
                    TextureBaker.BakeTexture(this);
                }
                isBaking = false;
            }
            else
            {
                ShaderGraphBakerDebug.Log(LogType.Error, "Not available in the free version");
                return;
            }            
        }

        public void InitShaderBakerButton(UnityEngine.UIElements.Button button)
        {
            m_ShaderBakerButton = button;

            UpdateShaderBakerButtonName();
        }
        public void UpdateShaderBakerButtonName()
        {
            if (m_ShaderBakerButton != null)
            {
                m_ShaderBakerButton.text = "Bake Shader";

                m_ShaderBakerButton.SetEnabled(false);
            }
        }
        public void ShaderBakerButtonCallback()
        {
            //not available in free version
        }
    }
}
