using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FanXing.Data;

/// <summary>
/// 策划配置编辑器窗口，提供可视化的配置数据编辑功能
/// 作者：黄畅修
/// 创建时间：2025-07-20
/// </summary>
namespace FanXing.Editor
{
    public class ConfigEditorWindow : FXEditorBase
    {
        #region 菜单项
        [MenuItem(MENU_ROOT + "配置编辑器/策划配置工具", false, 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<ConfigEditorWindow>("策划配置工具");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }
        #endregion

        #region 字段定义
        private int _selectedTabIndex = 0;
        private readonly string[] _tabNames = { "NPC配置", "任务配置", "商店配置", "作物配置", "技能配置" };
        
        // NPC配置
        private List<NPCConfigData> _npcConfigs = new List<NPCConfigData>();
        private NPCConfigData _selectedNPC;
        private int _selectedNPCIndex = -1;
        
        // 任务配置 - 预留字段，待后续实现
        // private List<QuestConfigData> _questConfigs = new List<QuestConfigData>();
        // private QuestConfigData _selectedQuest;
        // private int _selectedQuestIndex = -1;

        // 商店配置 - 预留字段，待后续实现
        // private List<ShopConfigData> _shopConfigs = new List<ShopConfigData>();
        // private ShopConfigData _selectedShop;
        // private int _selectedShopIndex = -1;

        // 作物配置 - 预留字段，待后续实现
        // private List<CropConfigData> _cropConfigs = new List<CropConfigData>();
        // private CropConfigData _selectedCrop;
        // private int _selectedCropIndex = -1;

        // 技能配置 - 预留字段，待后续实现
        // private List<SkillConfigData> _skillConfigs = new List<SkillConfigData>();
        // private SkillConfigData _selectedSkill;
        // private int _selectedSkillIndex = -1;
        #endregion

        #region 生命周期
        protected override void LoadData()
        {
            LoadNPCConfigs();
            // TODO: 后续实现其他配置的加载
            // LoadQuestConfigs();
            // LoadShopConfigs();
            // LoadCropConfigs();
            // LoadSkillConfigs();
        }

        protected override void SaveData()
        {
            SaveNPCConfigs();
            // TODO: 后续实现其他配置的保存
            // SaveQuestConfigs();
            // SaveShopConfigs();
            // SaveCropConfigs();
            // SaveSkillConfigs();
        }
        #endregion

        #region GUI绘制
        protected override void OnGUI()
        {
            DrawTitle("繁星Demo - 策划配置工具");
            
            // 绘制标签页
            _selectedTabIndex = GUILayout.Toolbar(_selectedTabIndex, _tabNames);
            
            EditorGUILayout.Space(10);
            
            // 绘制对应的配置界面
            switch (_selectedTabIndex)
            {
                case 0: DrawNPCConfigTab(); break;
                case 1: DrawQuestConfigTab(); break;
                case 2: DrawShopConfigTab(); break;
                case 3: DrawCropConfigTab(); break;
                case 4: DrawSkillConfigTab(); break;
            }
        }
        #endregion

        #region NPC配置
        private void DrawNPCConfigTab()
        {
            EditorGUILayout.BeginHorizontal();
            
            // 左侧列表
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            DrawHeader("NPC列表");
            
            DrawButtonGroup(
                ("新建NPC", CreateNewNPC),
                ("删除NPC", DeleteSelectedNPC)
            );
            
            EditorGUILayout.Space(5);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(400));
            for (int i = 0; i < _npcConfigs.Count; i++)
            {
                bool isSelected = i == _selectedNPCIndex;
                GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
                
                if (GUILayout.Button($"{_npcConfigs[i].npcName} (ID:{_npcConfigs[i].npcId})", GUILayout.Height(25)))
                {
                    _selectedNPCIndex = i;
                    _selectedNPC = _npcConfigs[i];
                }
                
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
            
            // 右侧详情
            EditorGUILayout.BeginVertical();
            if (_selectedNPC != null)
            {
                DrawNPCDetails();
            }
            else
            {
                EditorGUILayout.LabelField("请选择一个NPC进行编辑", EditorStyles.centeredGreyMiniLabel);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawNPCDetails()
        {
            DrawHeader("NPC详细配置");
            
            _selectedNPC.npcId = EditorGUILayout.IntField("NPC ID", _selectedNPC.npcId);
            _selectedNPC.npcName = EditorGUILayout.TextField("NPC名称", _selectedNPC.npcName);
            _selectedNPC.npcType = (NPCType)EditorGUILayout.EnumPopup("NPC类型", _selectedNPC.npcType);
            _selectedNPC.dialogueText = EditorGUILayout.TextArea(_selectedNPC.dialogueText, GUILayout.Height(60));
            _selectedNPC.position = EditorGUILayout.Vector3Field("位置", _selectedNPC.position);
            _selectedNPC.isInteractable = EditorGUILayout.Toggle("可交互", _selectedNPC.isInteractable);
            
            EditorGUILayout.Space(10);
            DrawButtonGroup(
                ("保存配置", SaveNPCConfigs),
                ("导出JSON", () => ExportJsonConfig(_npcConfigs, "npc_config"))
            );
        }

        private void CreateNewNPC()
        {
            var newNPC = new NPCConfigData
            {
                npcId = _npcConfigs.Count + 1,
                npcName = "新NPC",
                npcType = NPCType.Merchant,
                dialogueText = "你好，我是新NPC。",
                position = Vector3.zero,
                isInteractable = true
            };
            
            _npcConfigs.Add(newNPC);
            _selectedNPCIndex = _npcConfigs.Count - 1;
            _selectedNPC = newNPC;
        }

        private void DeleteSelectedNPC()
        {
            if (_selectedNPCIndex >= 0 && _selectedNPCIndex < _npcConfigs.Count)
            {
                if (ShowConfirmDialog("删除确认", $"确定要删除NPC '{_selectedNPC.npcName}' 吗？"))
                {
                    _npcConfigs.RemoveAt(_selectedNPCIndex);
                    _selectedNPCIndex = -1;
                    _selectedNPC = null;
                }
            }
        }
        #endregion

        #region 任务配置
        private void DrawQuestConfigTab()
        {
            EditorGUILayout.LabelField("任务配置功能开发中...", EditorStyles.centeredGreyMiniLabel);
            // TODO: 实现任务配置界面
        }
        #endregion

        #region 商店配置
        private void DrawShopConfigTab()
        {
            EditorGUILayout.LabelField("商店配置功能开发中...", EditorStyles.centeredGreyMiniLabel);
            // TODO: 实现商店配置界面
        }
        #endregion

        #region 作物配置
        private void DrawCropConfigTab()
        {
            EditorGUILayout.LabelField("作物配置功能开发中...", EditorStyles.centeredGreyMiniLabel);
            // TODO: 实现作物配置界面
        }
        #endregion

        #region 技能配置
        private void DrawSkillConfigTab()
        {
            EditorGUILayout.LabelField("技能配置功能开发中...", EditorStyles.centeredGreyMiniLabel);
            // TODO: 实现技能配置界面
        }
        #endregion

        #region 数据加载保存
        private void LoadNPCConfigs()
        {
            _npcConfigs = ImportJsonConfig<List<NPCConfigData>>("npc_config") ?? new List<NPCConfigData>();
        }

        private void SaveNPCConfigs()
        {
            ExportJsonConfig(_npcConfigs, "npc_config");
            ShowSuccessMessage("NPC配置已保存！");
        }

        private void LoadQuestConfigs() { /* TODO */ }
        private void SaveQuestConfigs() { /* TODO */ }
        private void LoadShopConfigs() { /* TODO */ }
        private void SaveShopConfigs() { /* TODO */ }
        private void LoadCropConfigs() { /* TODO */ }
        private void SaveCropConfigs() { /* TODO */ }
        private void LoadSkillConfigs() { /* TODO */ }
        private void SaveSkillConfigs() { /* TODO */ }
        #endregion
    }

    #region 配置数据结构
    [System.Serializable]
    public class NPCConfigData
    {
        public int npcId;
        public string npcName;
        public NPCType npcType;
        public string dialogueText;
        public Vector3 position;
        public bool isInteractable;
    }

    [System.Serializable]
    public class QuestConfigData
    {
        public int questId;
        public string questName;
        public QuestType questType;
        public string description;
        public int rewardGold;
        public int rewardExp;
    }

    [System.Serializable]
    public class ShopConfigData
    {
        public int shopId;
        public string shopName;
        public ShopType shopType;
        public int rentCost;
        public Vector3 position;
    }

    [System.Serializable]
    public class CropConfigData
    {
        public int cropId;
        public string cropName;
        public CropType cropType;
        public float growthTime;
        public int sellPrice;
    }

    [System.Serializable]
    public class SkillConfigData
    {
        public int skillId;
        public string skillName;
        public SkillType skillType;
        public int manaCost;
        public float cooldown;
    }
    #endregion
}
