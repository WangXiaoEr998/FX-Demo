using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FanXing.Data;
using System.Linq;

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
        // 持久化的 NPC Id 编辑字符串，避免每帧被重置导致无法显示非法输入及 HelpBox
        private string _npcIdInput = string.Empty;
        private bool _saveNPCSuccess = false;
        private bool _saveNPCConfig = true;

        // 任务配置
        private List<QuestConfigData> _questConfigs = new List<QuestConfigData>();
        private QuestConfigData _selectedQuest;
        private int _selectedQuestIndex = -1;
        private string _questIdInput = string.Empty;
        private bool _saveQuestSuccess = false;
        private bool _saveQuestConfig = true;

        // 商店配置
        private List<ShopConfigData> _shopConfigs = new List<ShopConfigData>();
        private ShopConfigData _selectedShop;
        private int _selectedShopIndex = -1;
        private string _shopIdInput = string.Empty;
        private bool _saveShopSuccess = false;
        private bool _saveShopConfig = true;

        // 作物配置
        private List<CropConfigData> _cropConfigs = new List<CropConfigData>();
        private CropConfigData _selectedCrop;
        private int _selectedCropIndex = -1;
        private string _cropIdInput = string.Empty;
        private bool _saveCropSuccess = false;
        private bool _saveCropConfig = true;

        // 技能配置
        private List<SkillConfigData> _skillConfigs = new List<SkillConfigData>();
        private SkillConfigData _selectedSkill;
        private int _selectedSkillIndex = -1;
        private string _skillIdInput = string.Empty;
        private bool _saveSkillSuccess = false;
        private bool _saveSkillConfig = true;
        #endregion

        protected override void OnDisable()
        {
            base.OnDisable();
            ShowSaveResult();
        }

        #region 生命周期
        protected override void LoadData()
        {
            LoadNPCConfigs();
            LoadQuestConfigs();
            LoadShopConfigs();
            LoadCropConfigs();
            LoadSkillConfigs();
        }
        
        protected override void SaveData()
        {
            SaveNPCConfigs(_saveNPCConfig, false);
            SaveQuestConfigs(_saveQuestConfig, false);
            SaveShopConfigs(_saveShopConfig, false);
            SaveCropConfigs(_saveCropConfig, false);
            SaveSkillConfigs(_saveSkillConfig, false);
        }
        #endregion

        #region GUI绘制
        protected override void OnGUI()
        {
            base.OnGUI();
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
                    _npcIdInput = _selectedNPC.npcId.ToString(); // 切换时同步编辑缓存
                    GUI.FocusControl(null); // 取消焦点，避免按钮被选中时无法输入
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
                EditorGUI.indentLevel -= 17;
                EditorGUILayout.LabelField("请选择一个NPC进行编辑", EditorStyles.centeredGreyMiniLabel);
                EditorGUI.indentLevel += 17;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }
        private void DrawNPCDetails()
        {
            DrawHeader("NPC详细配置");

            if (string.IsNullOrEmpty(_npcIdInput))
                _npcIdInput = _selectedNPC.npcId.ToString();
            _npcIdInput = EditorGUILayout.TextField(new GUIContent("NPC Id", "请输入正整数作为NPC的唯一标识"), _npcIdInput);
            bool npcIdValid = int.TryParse(_npcIdInput, out int parsedId) && parsedId > 0;
            if (npcIdValid)
            {
                bool duplicate = _npcConfigs.Any(n => n != _selectedNPC && n.npcId == parsedId);
                if (duplicate)
                {
                    _saveNPCConfig = false;
                    EditorGUILayout.HelpBox($"ID {parsedId} 已存在，不能重复", MessageType.Error);
                }
                else
                {
                    _saveNPCConfig = true;
                    _selectedNPC.npcId = parsedId; // 只有合法且不重复才写回
                }
            }
            else
            {
                _saveNPCConfig = false;
                EditorGUILayout.HelpBox("NPC Id必须是正整数", MessageType.Error);
            }
            _selectedNPC.npcName = EditorGUILayout.TextField("NPC名称", _selectedNPC.npcName);
            _selectedNPC.npcType = (NPCType)EditorGUILayout.EnumPopup("NPC类型", _selectedNPC.npcType);
            _selectedNPC.dialogueText = EditorGUILayout.TextArea(_selectedNPC.dialogueText, GUILayout.Height(60));
            _selectedNPC.position = EditorGUILayout.Vector3Field("位置", _selectedNPC.position);
            _selectedNPC.isInteractable = EditorGUILayout.Toggle("可交互", _selectedNPC.isInteractable);

            EditorGUILayout.Space(10);
            DrawButtonGroup(
                ("保存配置", () =>
                {
                    if (_saveNPCConfig)
                        SaveNPCConfigs(true);
                    else
                        EditorUtility.DisplayDialog("错误", "Id格式输入错误", "确定");
                }
            ),
                ("导出JSON", () => ExportJsonConfig(_npcConfigs, "npc_config"))
            );
        }
        private int GetNextNPCId()
        {
            if (_npcConfigs.Count == 0) return 1;
            return _npcConfigs.Max(n => n.npcId) + 1;
        }
        private void CreateNewNPC()
        {
            var newNPC = new NPCConfigData
            {
                npcId = GetNextNPCId(),
                npcName = "新NPC",
                npcType = NPCType.Merchant,
                dialogueText = "你好,我是新NPC。",
                position = Vector3.zero,
                isInteractable = true
            };

            _npcConfigs.Add(newNPC);
            _selectedNPCIndex = _npcConfigs.Count - 1;
            _selectedNPC = newNPC;
            _npcIdInput = newNPC.npcId.ToString();
            GUI.FocusControl(null);
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
                    _npcIdInput = string.Empty; // 清空编辑缓存
                }
            }
        }
        #endregion

        #region 任务配置
        private void DrawQuestConfigTab()
        {
            EditorGUILayout.BeginHorizontal();
            // 左侧列表
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            DrawHeader("任务列表");
            DrawButtonGroup(
                ("创建任务", CreateNewQuest),
                ("删除任务", DeleteSelectedQuest)
                );
            EditorGUILayout.Space(5);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(400));
            for (int i = 0; i < _questConfigs.Count; i++)
            {
                bool isSelected = i == _selectedQuestIndex;
                GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
                // 按钮生成和交互
                if (GUILayout.Button($"{_questConfigs[i].questName}(ID:{_questConfigs[i].questId})", GUILayout.Height(25)))
                {
                    _selectedQuestIndex = i;
                    _selectedQuest = _questConfigs[i];
                    _questIdInput = _questConfigs[i].questId.ToString();
                    GUI.FocusControl(null);
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            // 右侧详情
            EditorGUILayout.BeginVertical();
            if (_selectedQuest != null)
            {
                DrawQuestDetails();
            }
            else
            {
                EditorGUI.indentLevel -= 17;
                EditorGUILayout.LabelField("请选择一个任务进行编辑", EditorStyles.centeredGreyMiniLabel); // 用于显示灰色、居中、小号的辅助性文本标签
                EditorGUI.indentLevel += 17;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

        }
        private void DrawQuestDetails()
        {
            DrawHeader("任务详细配置");
            if (string.IsNullOrEmpty(_questIdInput))
                _questIdInput = _selectedQuest.questId.ToString();
            _questIdInput = EditorGUILayout.TextField(new GUIContent("任务Id", "请输入正整数作为任务的唯一标识"), _questIdInput);
            bool questIdValid = int.TryParse(_questIdInput, out int parsedId) && parsedId > 0;
            if (questIdValid)
            {
                bool duplicate = _questConfigs.Any(q => q != _selectedQuest && q.questId == parsedId);
                if (duplicate)
                {
                    _saveQuestConfig = false;
                    EditorGUILayout.HelpBox($"ID{parsedId}已存在，不能重复", MessageType.Error);
                }
                else
                {
                    _saveQuestConfig = true;
                    _selectedQuest.questId = parsedId;
                }
            }
            else
            {
                _saveQuestConfig = false;
                EditorGUILayout.HelpBox("任务 Id必须是正整数", MessageType.Error);
            }
            _selectedQuest.questName = EditorGUILayout.TextField("任务名称", _selectedQuest.questName);
            _selectedQuest.questType = (QuestType)EditorGUILayout.EnumPopup("任务类型", _selectedQuest.questType);
            _selectedQuest.description = EditorGUILayout.TextArea(_selectedQuest.description, GUILayout.Height(80));
            _selectedQuest.rewardGold = EditorGUILayout.IntField("任务奖励", _selectedQuest.rewardGold);
            _selectedQuest.rewardExp = EditorGUILayout.IntField("任务经验", _selectedQuest.rewardExp);

            EditorGUILayout.Space(10);
            DrawButtonGroup(
             ("保存配置", () =>
             {
                 if (_saveQuestConfig)
                     SaveQuestConfigs(true);
                 else
                     EditorUtility.DisplayDialog("错误", "Id格式输入错误", "确定");
             }
            ),
             ("导出JSON", () => ExportJsonConfig(_questConfigs, "quest_config"))
         );
        }
        private int GetNextQuestId()
        {
            if (_questConfigs.Count == 0) return 1;
            return _questConfigs.Max(q => q.questId) + 1;
        }
        private void CreateNewQuest()
        {
            var newQuest = new QuestConfigData
            {
                questId = GetNextQuestId(),
                questName = "新任务",
                questType = QuestType.Side,
                description = "这是新的任务",
                rewardGold = 10,
                rewardExp = 100,
            };
            _questConfigs.Add(newQuest);
            _selectedQuestIndex = _questConfigs.Count - 1;
            _selectedQuest = newQuest;
            _questIdInput = newQuest.questId.ToString();
            GUI.FocusControl(null);
        }
        private void DeleteSelectedQuest()
        {
            if (_selectedQuestIndex >= 0 && _selectedQuestIndex < _questConfigs.Count)
            {
                if (ShowConfirmDialog("删除确认", $"确认要删除任务'{_selectedQuest.questName}'吗？"))
                {
                    _questConfigs.RemoveAt(_selectedQuestIndex);
                    _selectedQuestIndex = -1;
                    _selectedQuest = null;
                    _questIdInput = string.Empty;
                }
            }
        }
        #endregion

        #region 商品配置
        private void DrawShopConfigTab()
        {
            EditorGUILayout.BeginHorizontal();

            // 左侧列表
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            DrawHeader("商品列表");
            DrawButtonGroup(
                ("新建商品", CreateNewShop),
                ("删除商品", DeleteSelectedShop)
                );
            EditorGUILayout.Space(5);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(400));
            for (int i = 0; i < _shopConfigs.Count; i++)
            {
                bool isSelected = i == _selectedShopIndex;
                GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
                if (GUILayout.Button($"{_shopConfigs[i].shopName}(ID:{_shopConfigs[i].shopId})", GUILayout.Height(25)))
                {
                    _selectedShopIndex = i;
                    _selectedShop = _shopConfigs[i];
                    _shopIdInput = _shopConfigs[i].shopId.ToString();
                    GUI.FocusControl(null); // 取消焦点，避免按钮被选中时无法输入
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            // 右侧详情
            EditorGUILayout.BeginVertical();
            if (_selectedShop != null)
            {
                DrawShopDetails();
            }
            else
            {
                EditorGUI.indentLevel -= 17;
                EditorGUILayout.LabelField("请选择一个商品进行编辑", EditorStyles.centeredGreyMiniLabel);
                EditorGUI.indentLevel += 17;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

        }
        private void DrawShopDetails()
        {
            DrawHeader("商品详细配置");
            if (string.IsNullOrEmpty(_shopIdInput))
                _shopIdInput = _selectedShop.shopId.ToString();
            _shopIdInput = EditorGUILayout.TextField(new GUIContent("商品 Id", "请输入正整数作为商品的唯一标识"), _shopIdInput);
            bool shopIdValid = int.TryParse(_shopIdInput, out int parsedId) && parsedId > 0;
            if (shopIdValid)
            {
                bool duplicate = _shopConfigs.Any(s => s != _selectedShop && s.shopId == parsedId);
                if (duplicate)
                {
                    _saveShopConfig = false;
                    EditorGUILayout.HelpBox($"ID{parsedId}已存在，不能重复", MessageType.Error);
                }
                else
                {
                    _saveShopConfig = true;
                    _selectedShop.shopId = parsedId;
                }
            }
            else
            {
                _saveShopConfig = false;
                EditorGUILayout.HelpBox("商品 Id必须是正整数", MessageType.Error);
            }
            _selectedShop.shopName = EditorGUILayout.TextField("商品名称", _selectedShop.shopName);
            _selectedShop.shopType = (ShopType)EditorGUILayout.EnumPopup("商品类型", _selectedShop.shopType);
            _selectedShop.rentCost = EditorGUILayout.IntField("商品价格", _selectedShop.rentCost);
            _selectedShop.position = EditorGUILayout.Vector3Field("商品位置", _selectedShop.position);

            EditorGUILayout.Space(10);
            DrawButtonGroup(
                ("保存配置", () =>
                {
                    if (_saveShopConfig)
                        SaveShopConfigs(true);
                    else
                        EditorUtility.DisplayDialog("错误", "Id格式输入错误", "确定");
                }
            ),
                ("导出JSON", () => ExportJsonConfig(_shopConfigs, "shop_config"))
                );
        }
        private void DeleteSelectedShop()
        {
            if (_selectedShopIndex >= 0 && _selectedShopIndex < _shopConfigs.Count)
            {
                if (ShowConfirmDialog("删除确认", $"确认要删除商品'{_selectedShop.shopName}'吗？"))
                {
                    _shopConfigs.RemoveAt(_selectedShopIndex);
                    _selectedShopIndex = -1;
                    _selectedShop = null;
                    _shopIdInput = string.Empty;
                }
            }
        }
        private int GetNextShopId()
        {
            if (_shopConfigs.Count == 0) return 1;
            return _shopConfigs.Max(s => s.shopId) + 1;
        }
        private void CreateNewShop()
        {
            var newShop = new ShopConfigData
            {
                shopId = GetNextShopId(),
                shopName = "新商品",
                shopType = ShopType.General,
                rentCost = 10,
                position = Vector3.zero
            };
            _shopConfigs.Add(newShop);
            _selectedShop = newShop;
            _selectedShopIndex = _shopConfigs.Count - 1;
            _shopIdInput = newShop.shopId.ToString();
            GUI.FocusControl(null);
        }
        #endregion

        #region 作物配置
        private void DrawCropConfigTab()
        {
            EditorGUILayout.BeginHorizontal();
            // 左侧列表
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            DrawHeader("作物列表");
            DrawButtonGroup(
                ("新建作物", CreateNewCrop),
                ("删除作物", DeleteSelectedCrop)
                );
            EditorGUILayout.Space(5);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(400));
            for (int i = 0; i < _cropConfigs.Count; i++)
            {
                bool isSelected = i == _selectedCropIndex;
                GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
                if (GUILayout.Button($"{_cropConfigs[i].cropName}(ID:{_cropConfigs[i].cropId})", GUILayout.Height(25)))
                {
                    _selectedCropIndex = i;
                    _selectedCrop = _cropConfigs[i];
                    _cropIdInput = _cropConfigs[i].cropId.ToString();
                    GUI.FocusControl(null);
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            // 右侧详情
            EditorGUILayout.BeginVertical();
            if (_selectedCrop != null)
            {
                DrawCropDetails();
            }
            else
            {
                EditorGUI.indentLevel -= 17;
                EditorGUILayout.LabelField("请选择一个作物进行编辑", EditorStyles.centeredGreyMiniLabel);
                EditorGUI.indentLevel += 17;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        private void DrawCropDetails()
        {
            DrawHeader("作物详情配置");
            if(string.IsNullOrEmpty(_cropIdInput))
                _cropIdInput = _selectedCrop.cropId.ToString();
            _cropIdInput = EditorGUILayout.TextField(new GUIContent("作物Id", "请输入正整数作为作物的唯一标识"), _cropIdInput);
            bool cropIdValid = int.TryParse(_cropIdInput,out int parsedId) && parsedId > 0;
            if (cropIdValid)
            {
                bool duplicate = _cropConfigs.Any(c => c != _selectedCrop && c.cropId == parsedId);
                if (duplicate)
                {
                    _saveCropConfig = false;
                    EditorGUILayout.HelpBox($"Id{parsedId}已存在，不能重复", MessageType.Error);
                }
                else
                {
                    _saveCropConfig = true;
                    _selectedCrop.cropId = parsedId;
                }
            }
            else
            {
                _saveCropConfig= false;
                EditorGUILayout.HelpBox("作物Id必须是正整数", MessageType.Error);
            }
            _selectedCrop.cropName = EditorGUILayout.TextField("作物名称", _selectedCrop.cropName);
            _selectedCrop.cropType = (CropType)EditorGUILayout.EnumPopup("作物类型", _selectedCrop.cropType);
            _selectedCrop.growthTime = EditorGUILayout.FloatField("作物生长时间", _selectedCrop.growthTime);
            _selectedCrop.sellPrice = EditorGUILayout.IntField("作物价格", _selectedCrop.sellPrice);

            EditorGUILayout.Space(10);
            DrawButtonGroup(
                ("保存配置", () =>
                {
                    if (_saveCropConfig)
                        SaveCropConfigs(true);
                    else
                        EditorUtility.DisplayDialog("错误", "Id格式输入错误", "确定");
                }
            ),
                ("导出JSON", () => ExportJsonConfig(_cropConfigs, "crop_config"))
                );
        }
        private int GetNextCropId()
        {
            if (_cropConfigs.Count == 0) return 1;
            return _cropConfigs.Max(c => c.cropId) + 1;
        }
        private void CreateNewCrop()
        {
            var newCrop = new CropConfigData
            {
                cropId = GetNextCropId(),
                cropName = "新作物",
                cropType = CropType.None,
                growthTime = 0,
                sellPrice = 0
            };
            _cropConfigs.Add(newCrop);
            _selectedCropIndex = _cropConfigs.Count - 1;
            _selectedCrop = newCrop;
            _cropIdInput = newCrop.cropId.ToString();
            GUI.FocusControl(null);
        }
        private void DeleteSelectedCrop()
        {
            if (_selectedCropIndex >= 0 && _selectedCropIndex < _cropConfigs.Count)
            {
                if (ShowConfirmDialog("删除确认", $"确认要删除作物'{_selectedCrop.cropName}'吗?"))
                {
                    _cropConfigs.RemoveAt(_selectedCropIndex);
                    _selectedCropIndex = -1;
                    _selectedCrop = null;
                    _cropIdInput = string.Empty;
                }
            }
        }
        #endregion

        #region 技能配置
        private void DrawSkillConfigTab()
        {
            EditorGUILayout.BeginHorizontal();
            // 左侧列表
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            DrawHeader("技能列表");
            DrawButtonGroup(
                ("新建技能", CreateNewSkill),
                ("删除技能", DeleteSelectedSkill)
                );
            EditorGUILayout.Space(5);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(400));
            for (int i = 0; i < _skillConfigs.Count; i++)
            {
                bool isSelected = i == _selectedSkillIndex;
                GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
                if (GUILayout.Button($"{_skillConfigs[i].skillName}(ID:{_skillConfigs[i].skillId})", GUILayout.Height(25)))
                {
                    _selectedSkillIndex = i; 
                    _selectedSkill = _skillConfigs[i];
                    _skillIdInput = _skillConfigs[i].skillId.ToString();
                    GUI.FocusControl(null);
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // 右侧
            EditorGUILayout.BeginVertical();
            if (_selectedSkill != null)
            {
                DrawSkillDetails();
            }
            else
            {
                EditorGUI.indentLevel -= 17;
                EditorGUILayout.LabelField("请选择一个技能进行编辑", EditorStyles.centeredGreyMiniLabel);
                EditorGUI.indentLevel += 17;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        private void DrawSkillDetails()
        {
            DrawHeader("技能详细配置");
            if (string.IsNullOrEmpty(_skillIdInput))
                _skillIdInput = _selectedSkill.skillId.ToString();
            _skillIdInput = EditorGUILayout.TextField(new GUIContent("技能Id", "请输入正整数作为技能的唯一标识"), _skillIdInput);
            bool skillIdValid = int.TryParse(_skillIdInput,out int parsedId) && parsedId > 0;
            if (skillIdValid)
            {
                // 解析成功后需要判断parsedId是否已经重复
                bool duplicate = _skillConfigs.Any(s => s != _selectedSkill && s.skillId == parsedId);
                if (duplicate)
                {
                    _saveSkillConfig = false;
                    EditorGUILayout.HelpBox($"Id{parsedId}已存在，不能重复", MessageType.Error);
                }
                else
                {
                    _saveSkillConfig= true;
                    _selectedSkill.skillId = parsedId;
                }
            }
            else
            {
                _saveSkillConfig = false;
                EditorGUILayout.HelpBox("技能Id必须是正整数", MessageType.Error);
            }
            _selectedSkill.skillName = EditorGUILayout.TextField("技能名称", _selectedSkill.skillName);
            _selectedSkill.skillType = (SkillType)EditorGUILayout.EnumPopup("技能类型", _selectedSkill.skillType);
            _selectedSkill.manaCost = EditorGUILayout.IntField("技能消耗", _selectedSkill.manaCost);
            _selectedSkill.cooldown = EditorGUILayout.FloatField("冷却时间", _selectedSkill.cooldown);
            EditorGUILayout.Space(10);
            DrawButtonGroup(
                ("保存配置", () =>
                {
                    if (_saveSkillConfig)
                        SaveSkillConfigs(true);
                    else
                        EditorUtility.DisplayDialog("错误", "Id格式输入错误", "确定");
                }
            ),
                ("导出JSON", () => ExportJsonConfig(_skillConfigs, "skill_config"))
            );
        }
        private int GetNextSkillId()
        {
            if (_skillConfigs.Count == 0) return 1;
            return _skillConfigs.Max(s => s.skillId) + 1;
        }
        private void CreateNewSkill()
        {
            var newSkill = new SkillConfigData
            {
                skillId = GetNextSkillId(),
                skillName = "新技能",
                skillType = SkillType.Active,
                manaCost = 50,
                cooldown = 3f
            };
            _skillConfigs.Add(newSkill);
            _selectedSkillIndex = _skillConfigs.Count - 1;
            _selectedSkill = newSkill;
            _skillIdInput = newSkill.skillId.ToString();
            GUI.FocusControl(null);
        }
        private void DeleteSelectedSkill()
        {
            if (_selectedSkillIndex >= 0 && _selectedSkillIndex < _skillConfigs.Count)
            {
                if (ShowConfirmDialog("确认删除", $"确认要删除技能'{_selectedSkill.skillName}'吗？"))
                {
                    _skillConfigs.RemoveAt(_selectedSkillIndex);
                    _selectedSkillIndex = -1;
                    _selectedSkill = null;
                    _skillIdInput = string.Empty;
                }
            }
        }
        #endregion

        #region 数据加载保存
        private void LoadNPCConfigs()
        {
            _npcConfigs = ImportJsonConfig<List<NPCConfigData>>("npc_config") ?? new List<NPCConfigData>();
        }

        private void SaveNPCConfigs(bool saveConfigs, bool showDialog = true)
        {
            if (saveConfigs)
            {
                ExportJsonConfig(_npcConfigs, "npc_config");
                if (showDialog)
                    ShowSuccessMessage("NPC配置已保存!");
                _saveNPCSuccess = true;
            }
        }

        private void LoadQuestConfigs()
        {
            _questConfigs = ImportJsonConfig<List<QuestConfigData>>("quest_config") ?? new List<QuestConfigData>();
        }
        private void SaveQuestConfigs(bool saveConfigs,bool showDialog = true)
        {
            if (saveConfigs)
            {
                ExportJsonConfig(_questConfigs, "quest_config");
                if (showDialog)
                    ShowSuccessMessage("任务配置已保存!");
                _saveQuestSuccess = true;
            }
        }
        private void LoadShopConfigs()
        {
            _shopConfigs = ImportJsonConfig<List<ShopConfigData>>("shop_config") ?? new List<ShopConfigData>();
        }
        private void SaveShopConfigs(bool saveConfigs,bool showDialog = true)
        {
            if (saveConfigs)
            {
                ExportJsonConfig(_shopConfigs, "shop_config");
                if (showDialog)
                    ShowSuccessMessage("商品配置已保存!");
                _saveShopSuccess = true;
            }
        }
        private void LoadCropConfigs()
        {
            _cropConfigs = ImportJsonConfig<List<CropConfigData>>("crop_config") ?? new List<CropConfigData>();
        }
        private void SaveCropConfigs(bool saveConfigs,bool showDialog = true)
        {
            if (saveConfigs)
            {
                ExportJsonConfig(_cropConfigs, "crop_config");
                if (showDialog)
                    ShowSuccessMessage("作物配置已保存!");
                _saveCropSuccess = true;
            }
        }
        private void LoadSkillConfigs()
        {
            _skillConfigs = ImportJsonConfig<List<SkillConfigData>>("skill_config") ?? new List<SkillConfigData>();
        }
        private void SaveSkillConfigs(bool saveConfigs,bool showDialog = true)
        {
            if (saveConfigs)
            {
                ExportJsonConfig(_skillConfigs, "skill_config");
                if (showDialog)
                    ShowSuccessMessage("技能配置已保存!");
                _saveSkillSuccess = true;
            }
        }
        #endregion

        #region 显示保存结果
        private void ShowSaveResult()
        {
            List<string> showErrorMessage = new List<string>();
            string showResult = null;
            if (_saveNPCSuccess && _saveCropSuccess && _saveQuestSuccess && _saveShopSuccess && _saveSkillSuccess)
                ShowSuccessMessage("配置均已保存");
            if (!_saveNPCSuccess && !_saveCropSuccess && !_saveQuestSuccess && !_saveShopSuccess && !_saveSkillSuccess)
            {
                ShowErrorMessage("配置全部保存失败");
                return;
            }
            if (!_saveNPCSuccess)
                showErrorMessage.Add("NPC配置");
            if (!_saveCropSuccess)
                showErrorMessage.Add("作物配置");
            if (!_saveQuestSuccess)
                showErrorMessage.Add("任务配置");
            if (!_saveShopSuccess)
                showErrorMessage.Add("商品配置");
            if (!_saveSkillSuccess)
                showErrorMessage.Add("技能配置");
            showResult = string.Join("、 ", showErrorMessage);
            ShowErrorMessage(showResult + "保存失败,其他保存成功");
        }
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
