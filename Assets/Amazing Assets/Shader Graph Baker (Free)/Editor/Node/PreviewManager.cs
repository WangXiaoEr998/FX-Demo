// Shader Graph Baker (Free) <https://u3d.as/3ycp>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Internal;


namespace AmazingAssets.ShaderGraphBakerFree.Editor
{    
    internal class PreviewManager
    {
        internal enum PropagationDirection
        {
            Upstream,
            Downstream
        }

        internal static void PropagateNodes(HashSet<AbstractMaterialNode> sources, PropagationDirection dir, HashSet<AbstractMaterialNode> result)
        {
            Stack<AbstractMaterialNode> m_TempNodeWave = new Stack<AbstractMaterialNode>();
            HashSet<AbstractMaterialNode> m_TempAddedToNodeWave = new HashSet<AbstractMaterialNode>();

            Action<AbstractMaterialNode> AddNextLevelNodesToWave =
            nextLevelNode =>
            {
                if (!m_TempAddedToNodeWave.Contains(nextLevelNode))
                {
                    m_TempNodeWave.Push(nextLevelNode);
                    m_TempAddedToNodeWave.Add(nextLevelNode);
                }
            };


            if (sources.Count > 0)
            {
                // NodeWave represents the list of nodes we still have to process and add to result
                m_TempNodeWave.Clear();
                m_TempAddedToNodeWave.Clear();
                foreach (var node in sources)
                {
                    m_TempNodeWave.Push(node);
                    m_TempAddedToNodeWave.Add(node);
                }

                while (m_TempNodeWave.Count > 0)
                {
                    var node = m_TempNodeWave.Pop();
                    if (node == null)
                        continue;

                    result.Add(node);

                    // grab connected nodes in propagation direction, add them to the node wave
                    ForeachConnectedNode(node, dir, AddNextLevelNodesToWave);
                }

                // clean up any temp data
                m_TempNodeWave.Clear();
                m_TempAddedToNodeWave.Clear();
            }
        }

        internal static void ForeachConnectedNode(AbstractMaterialNode node, PropagationDirection dir, Action<AbstractMaterialNode> action)
        {
            using (var tempEdges = PooledList<IEdge>.Get())
            using (var tempSlots = PooledList<MaterialSlot>.Get())
            {
                // Loop through all nodes that the node feeds into.
                if (dir == PropagationDirection.Downstream)
                    node.GetOutputSlots(tempSlots);
                else
                    node.GetInputSlots(tempSlots);

                foreach (var slot in tempSlots)
                {
                    // get the edges out of each slot
                    tempEdges.Clear();                            // and here we serialize another list, ouch!
                    node.owner.GetEdges(slot.slotReference, tempEdges);
                    foreach (var edge in tempEdges)
                    {
                        // We look at each node we feed into.
                        var connectedSlot = (dir == PropagationDirection.Downstream) ? edge.inputSlot : edge.outputSlot;
                        var connectedNode = connectedSlot.node;

                        action(connectedNode);
                    }
                }
            }
        }

        internal static void CollectPreviewProperties(GraphData graphData, IEnumerable<AbstractMaterialNode> nodesToCollect, PooledList<PreviewProperty> perMaterialPreviewProperties, MaterialPropertyBlock sharedPreviewPropertyBlock)
        {
            using (var tempPreviewProps = PooledList<PreviewProperty>.Get())
            {
                // collect from all of the changed nodes
                foreach (var propNode in nodesToCollect)
                    propNode.CollectPreviewMaterialProperties(tempPreviewProps);

                // also grab all graph properties (they are updated every frame)
                foreach (var prop in graphData.properties)
                    tempPreviewProps.Add(prop.GetPreviewMaterialProperty());

                foreach (var previewProperty in tempPreviewProps)
                {
                    previewProperty.SetValueOnMaterialPropertyBlock(sharedPreviewPropertyBlock);

                    // virtual texture assignments must be pushed to the materials themselves (MaterialPropertyBlocks not supported)
                    if ((previewProperty.propType == PropertyType.VirtualTexture) &&
                        (previewProperty.vtProperty?.value?.layers != null))
                    {
                        perMaterialPreviewProperties.Add(previewProperty);
                    }
                }
            }
        }

        internal static void AssignPerMaterialPreviewProperties(Material mat, List<PreviewProperty> perMaterialPreviewProperties)
        {
            foreach (var prop in perMaterialPreviewProperties)
            {
                switch (prop.propType)
                {
                    case PropertyType.VirtualTexture:

                        // setup the VT textures on the material
                        bool setAnyTextures = false;
                        var vt = prop.vtProperty.value;
                        for (int layer = 0; layer < vt.layers.Count; layer++)
                        {
                            var texture = vt.layers[layer].layerTexture?.texture;
                            int propIndex = mat.shader.FindPropertyIndex(vt.layers[layer].layerRefName);
                            if (propIndex != -1)
                            {
                                mat.SetTexture(vt.layers[layer].layerRefName, texture);
                                setAnyTextures = true;
                            }
                        }
                        // also put in a request for the VT tiles, since preview rendering does not have feedback enabled
                        if (setAnyTextures)
                        {
#if ENABLE_VIRTUALTEXTURES
                            int stackPropertyId = Shader.PropertyToID(prop.vtProperty.referenceName);
                            try
                            {
                                // Ensure we always request the mip sized 256x256
                                int width, height;
                                UnityEngine.Rendering.VirtualTexturing.Streaming.GetTextureStackSize(mat, stackPropertyId, out width, out height);
                                int textureMip = (int)Math.Max(Mathf.Log(width, 2f), Mathf.Log(height, 2f));
                                const int baseMip = 8;
                                int mip = Math.Max(textureMip - baseMip, 0);
                                UnityEngine.Rendering.VirtualTexturing.Streaming.RequestRegion(mat, stackPropertyId, new Rect(0.0f, 0.0f, 1.0f, 1.0f), mip, UnityEngine.Rendering.VirtualTexturing.System.AllMips);
                            }
                            catch (InvalidOperationException)
                            {
                                // This gets thrown when the system is in an indeterminate state (like a material with no textures assigned which can obviously never have a texture stack streamed).
                                // This is valid in this case as we're still authoring the material.
                            }
#endif // ENABLE_VIRTUALTEXTURES
                        }
                        break;
                }
            }
        }
    }
}
