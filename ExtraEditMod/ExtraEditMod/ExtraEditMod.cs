using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using CreatureGenerate;
using UnityEngine.SceneManagement; // コレ重要
using System.Text;
using UnityEngine;

namespace ExtraEditMod
{
    public class ExtraEditMod : MonoBehaviour
    {
        /// <summary>
        /// インスタンス
        /// </summary>
        private static ExtraEditMod _instance;

        #region メンバ変数

        /// <summary>
        /// Lobを追加する機能
        /// </summary>
        AddLob m_addLob = new AddLob();

        /// <summary>
        /// エネルギー追加
        /// </summary>
        AddEnergy m_addEnergy = new AddEnergy();

        /// <summary>
        /// オーダーを設定する機能
        /// </summary>
        OrderCreature m_orderCreature = new OrderCreature();

        /// <summary>
        /// エディットの状態設定保存
        /// </summary>
        EditSetting m_editSetting = new EditSetting();

        /// <summary>
        /// 選択する
        /// </summary>
        KeyValuePair<SefiraEnum, int> m_choiceKeyValue;

        /// <summary>
        /// 選択した箇所
        /// </summary>
        int m_choiceIndex;

        /// <summary>
        /// 選択した日
        /// </summary>
        int m_choiceDay;

        /// <summary>
        /// UIを表示するかどうか？
        /// </summary>
        public bool isEnableGui { get; set; }

        /// <summary>
        /// サブウインドウ表示
        /// </summary>
        bool isDrawSubWindow { get; set; }

        /// <summary>
        /// 選択できるアブノーマリティ一覧
        /// </summary>
        List<long> m_selectAbnormalityList = null;


        /// <summary>
        /// アブノーマリティの名前
        /// </summary>
        public static Dictionary<long, string> m_creatureName = new Dictionary<long, string>();

        /// <summary>
        /// アブノーマリティの画像
        /// </summary>
        public static Dictionary<long, Sprite> m_creatureSprite = new Dictionary<long, Sprite>();

        // スクロールの現在位置

        /// <summary>
        /// UIのメインウィンドウ用のスクロールの現在位置
        /// </summary>
        Vector2 mainScrollViewVector = Vector2.zero;

        /// <summary>
        /// UIのサブウィンドウ用のスクロールの現在位置
        /// </summary>
        Vector2 subScrollViewVector = Vector2.zero;

        /// <summary>
        /// スクロールビューの全体のRect範囲。position以上であれば、スクロールバーを操作して表示する 
        /// </summary>
        Rect scrollViewAllRect = new Rect(0, 0, 100, BUTTON_HEIGHT_INTARVAL * (OrderCreature.DAY_COUNT + 4));

        /// <summary>
        /// 現在のシーン
        /// </summary>
        string currentSceneName = "";

        /// <summary>
        /// このModを起動したシーン
        /// </summary>
        string modBootSceneName = "";

        /// <summary>
        /// ログを表示する
        /// </summary>
        public static string m_debuglog = "";

        /// <summary>
        /// デバッグログを表示するか？
        /// </summary>
        bool m_visibleDebugLog = false;

        #endregion

        #region  UIの基本値定数

        public const int BOX_X = 20;
        public const int BOX_Y = 20;
        public const int BOX_WIDTH = 350;
        public const int BOX_HEIGHT = 460;
        public const int BOX_IN_CONTENT_X = 25;

        public const int ADD_CONTENT_X = 5;
        public const int IMAGE_X = 10;

        public const int IMAGE_SIZE = 60;
        public const int UI_BETWEEN_HEIGHT = 10;
        public const int UI_BETWEEN_WIGHT = 5;
        public const int UI_PARTS_HIGHT = 30;

        public const int BUTTON_X = IMAGE_X + IMAGE_SIZE + 10;
        public const int BUTTON_WIDTH = 200;
        public const int BUTTON_HEIGHT_INTARVAL = 70;
        public const int UI_MARGIN = 20;
        #endregion

        /// <summary>
        /// インスタンス
        /// </summary>
        public static ExtraEditMod instance
        {
            get
            {
                if (ExtraEditMod._instance == null)
                {
                    ExtraEditMod._instance = new ExtraEditMod();
                }
                return ExtraEditMod._instance;
            }
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            if (_instance == null)
            {
                var go = new GameObject();
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<ExtraEditMod>();
                _instance.isEnableGui = true;
                _instance.InitCreature();
                _instance.m_orderCreature.Init();

                bool _enableAddLob;
                bool _enableAddEnergy;
                _instance.m_editSetting.LoadSettings(out _enableAddLob, out _enableAddEnergy, _instance.m_orderCreature);
                AddLob.enableAddLob = _enableAddLob;
                AddEnergy.enableAddEnergy = _enableAddEnergy;

                //このmodが起動したシーンと現状のシーンを登録
                _instance.modBootSceneName = SceneManager.GetActiveScene().name;
                _instance.currentSceneName = SceneManager.GetActiveScene().name;


                //modデバッグ用
                TextAsset textAsset = Resources.Load<TextAsset>("xml/CreatureGenInfo");
                m_debuglog += textAsset.text;
                m_debuglog += _instance.CreatureIdAndNameList();

            }
        }

        /// <summary>
        /// 更新ボタン
        /// </summary>
        void Update()
        {
            m_addLob.Update();
            m_addEnergy.Update();
            /*
            if (Input.GetKeyDown(KeyCode.O))
            {
                OutputImageManager.OutputAbnormaltyImage();
            }
            */

            if (Input.GetKeyDown(KeyCode.U) && isEnableGui)
            {
                m_visibleDebugLog = !m_visibleDebugLog;
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                isEnableGui = !isEnableGui;
            }
            //シーンが切り替わったとき非表示にする
            string temp = SceneManager.GetActiveScene().name;
            if (temp != currentSceneName)
            {
                isEnableGui = false;
                //このmodを起動したシーンなら表示とする
                if (modBootSceneName == temp)
                {
                    isEnableGui = true;
                }
                currentSceneName = temp;
            }
        }

        /// <summary>
        /// ID一覧を取得
        /// </summary>
        /// <returns></returns>
        public string CreatureIdAndNameList()
        {
            StringBuilder sb = new StringBuilder();
            long[] creatureList = new long[CreatureGenerateInfo.all.Length];
            Array.Copy(CreatureGenerateInfo.all, creatureList, CreatureGenerateInfo.all.Length);
            Array.Sort<long>(creatureList);

            foreach (var metaID in creatureList)
            {
                var typeInfo = CreatureTypeList.instance.GetData(metaID);
                string list = "ID:" + metaID + " code:" + typeInfo.codeId + " name:" + typeInfo.name + "\n";
                sb.Append(list);
            }
            return sb.ToString();
        }


        /// <summary>
        /// 文字列描画
        /// </summary>
        void OnGUI()
        {
            if (!isEnableGui) return;
            MainGUI();

            if (isDrawSubWindow)
            {
                SubGUI();
            }

            if (m_visibleDebugLog)
            {
                //modデバッグ用
                GUI.TextField(new Rect(10, 10, Screen.width - (BOX_X + BOX_WIDTH + 10), Screen.height - 20), m_debuglog);
            }
        }

        /// <summary>
        /// 初期化する
        /// </summary>
        public void InitCreature()
        {
            m_creatureSprite.Clear();
            foreach (var key in CreatureGenerateInfo.all)
            {
                var typeInfo = CreatureTypeList.instance.GetData(key);
                //string str = typeInfo.portraitSrc;

                string str = typeInfo.portraitSrcForcely;
                m_creatureSprite.Add(key, Resources.Load<Sprite>(str));
                m_creatureName.Add(key, typeInfo.collectionName);
            }
            m_creatureSprite.Add(0, Resources.Load<Sprite>("Sprites/Unit/creature/NoData"));
            m_creatureName.Add(0, "Random");
        }

        /// <summary>
        /// クリーチャーをセットする
        /// </summary>
        /// <param name="day"></param>
        /// <param name="cgm"></param>
        public static void SetCreature(object cgm)
        {
            _instance.m_orderCreature.SetCreature(cgm);
        }

        /// <summary>
        /// メインとして表示するウィンドウ
        /// </summary>
        void MainGUI()
        {
            GUI.backgroundColor = Color.red;

            var x = Screen.width - (BOX_X + BOX_WIDTH);
            var y = BOX_Y;
            GUI.Box(new Rect(x, y, BOX_WIDTH, BOX_HEIGHT), "ExtraEditMod");

            x += ADD_CONTENT_X;
            y = BOX_Y + UI_PARTS_HIGHT;

            var enableAddLob = AddLob.enableAddLob;

            //LOB追加機能
            AddLob.enableAddLob = GUI.Toggle(new Rect(x, y, 230, 20), AddLob.enableAddLob, "Addition of LOB by pressing L key");
            if (enableAddLob != AddLob.enableAddLob)
            {
                SaveSettings();
            }


            y += UI_PARTS_HIGHT;

            var enableAddEnergy = AddEnergy.enableAddEnergy;

            //エネルギー追加機能
            AddEnergy.enableAddEnergy = GUI.Toggle(new Rect(x, y, 230, 20), AddEnergy.enableAddEnergy, "Addition of Energy by pressing I key");
            if (enableAddEnergy != AddEnergy.enableAddEnergy)
            {
                SaveSettings();
            }


            y += UI_PARTS_HIGHT;

            // position : 表示位置
            Rect scrollViewRect = new Rect(x, y, BOX_WIDTH - 30, BUTTON_HEIGHT_INTARVAL * 4 + 10);

            //スクロールビュー配置
            mainScrollViewVector = GUI.BeginScrollView(scrollViewRect, mainScrollViewVector, scrollViewAllRect);

            y += BUTTON_HEIGHT_INTARVAL * 4 + 10 + UI_BETWEEN_HEIGHT * 2;

            int buttonAmount = 0;

            //スクロールの中身
            for (int i = 0; i < OrderCreature.DAY_COUNT; i++)
            {
                var sefira = m_orderCreature.m_dayToSefiraEnumDic[i];
                var level = (i % 5) + 1;
                var key = new KeyValuePair<SefiraEnum, int>(sefira, level);
                if (!m_orderCreature.m_creatureOlderDic.ContainsKey(key)) continue;
                var creatureIds = m_orderCreature.m_creatureOlderDic[key];

                int index = 0;
                bool breakFlag = false;
                foreach (var id in creatureIds)
                {
                    GUI.DrawTexture(new Rect(IMAGE_X, (buttonAmount * BUTTON_HEIGHT_INTARVAL), IMAGE_SIZE, IMAGE_SIZE), m_creatureSprite[id].texture);
                    var creatureCode = m_creatureName[id];
                    string label = string.Format("Day{0}_{1}:{2}", i + 1, index + 1, creatureCode);
                    if (GUI.Button(new Rect(BUTTON_X, (buttonAmount * BUTTON_HEIGHT_INTARVAL), BUTTON_WIDTH, IMAGE_SIZE), label))
                    {
                        m_choiceDay = i + 1;
                        m_choiceKeyValue = key;
                        m_choiceIndex = index;
                        isDrawSubWindow = true;
                        m_selectAbnormalityList = m_orderCreature.GetCreatureList();
                        subScrollViewVector = Vector2.zero;

                    }
                    buttonAmount++;
                    index++;
                    if (id == 0)
                    {
                        breakFlag = true;
                    }
                }
                if (breakFlag) break;
            }
            GUI.EndScrollView();

        }

        /// <summary>
        /// サブウインドウ
        /// </summary>
        void SubGUI()
        {
            var x = Screen.width - (BOX_X + BOX_WIDTH);
            var y = BOX_Y + BOX_HEIGHT;
            GUI.Box(new Rect(x, y, BOX_WIDTH, BOX_HEIGHT), string.Format("Day {0}:ChoiceAbnormality count:{1}", m_choiceDay, m_selectAbnormalityList.Count));


            x += ADD_CONTENT_X;
            y += UI_PARTS_HIGHT;

            // position : 表示位置
            Rect scrollViewRect = new Rect(x, y, BOX_WIDTH - 30, BUTTON_HEIGHT_INTARVAL * 4 + 10);

            var scrollViewAbnormalityListRect = new Rect(0, 0, 100, BUTTON_HEIGHT_INTARVAL * (m_selectAbnormalityList.Count + 4));

            //スクロールビュー配置
            subScrollViewVector = GUI.BeginScrollView(scrollViewRect, subScrollViewVector, scrollViewAbnormalityListRect);

            y += BUTTON_HEIGHT_INTARVAL * 4 + UI_MARGIN + UI_BETWEEN_HEIGHT * 2;

            int contensCount = 0;
            for (int i = -1; i < m_selectAbnormalityList.Count; i++)
            {
                var id = 0L;
                if (-1 < i)
                {
                    id = m_selectAbnormalityList[i];
                }
                GUI.DrawTexture(new Rect(IMAGE_X, (contensCount * BUTTON_HEIGHT_INTARVAL), IMAGE_SIZE, IMAGE_SIZE), m_creatureSprite[id].texture);
                var creatureCode = m_creatureName[id];
                string label = string.Format("{0}", creatureCode);
                if (GUI.Button(new Rect(BUTTON_X, (contensCount * BUTTON_HEIGHT_INTARVAL), BUTTON_WIDTH, IMAGE_SIZE), label))
                {
                    var list = m_orderCreature.m_creatureOlderDic[m_choiceKeyValue];
                    list[m_choiceIndex] = id;
                    isDrawSubWindow = false;
                    if (id == 0)
                    {
                        m_orderCreature.SetRandomFromBaseKv(m_choiceKeyValue, m_choiceIndex);
                    }
                    SaveSettings();
                }
                contensCount++;
            }
            GUI.EndScrollView();
        }
        /// <summary>
        /// セッティングを保存する
        /// </summary>
        void SaveSettings()
        {
            m_editSetting.SaveSettings(AddLob.enableAddLob, AddEnergy.enableAddEnergy, m_orderCreature);
        }
    }
}
