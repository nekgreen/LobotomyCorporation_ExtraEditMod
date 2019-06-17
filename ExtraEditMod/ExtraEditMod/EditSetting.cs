using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using System;
using System.Collections;




namespace ExtraEditMod
{
    class EditSetting
    {
        public class SettingData
        {
            public bool enableAddLob;
            public bool enableAddEnergy;
            public bool enableAlwayGetGift;
            public bool enableMalkutNote;
            public bool enableTaskManager;
            public OrderCreature orderCreature;
        }

        /// <summary>
        /// 例外時のメッセージ
        /// </summary>
        string exceptionMessage { get; set; }


        /// <summary>
        /// セッティングを読み込む
        /// </summary>
        /// <param name="enableAddLob"></param>
        /// <param name="_orderCreature"></param>
        public SettingData LoadSettings()
        {
            var settingData = new SettingData();
            settingData.enableAddLob = true;
            settingData.enableAddEnergy = false;
            settingData.enableAlwayGetGift = false;
            settingData.enableMalkutNote = true;
            settingData.enableTaskManager = true;
            settingData.orderCreature = new OrderCreature();

            string filePath = Application.persistentDataPath + "/" + EditSettingSaveData.m_saveDataName;
            string json = LoadSettingText(filePath);
            EditSettingSaveData saveData = JsonUtility.FromJson<EditSettingSaveData>(json);
            if (saveData != null)
            {
                settingData.enableAddLob = saveData.m_addMob;
                settingData.enableAddEnergy = saveData.m_addEnergy;
                settingData.enableAlwayGetGift = saveData.m_alwayGetGift;
                settingData.enableMalkutNote = saveData.m_malkutNote;
                settingData.enableTaskManager = saveData.m_taskManager;
                
                //アブノーマリティを設定する
                for (int i = 0; i < saveData.m_abnormaltyDataArray.Length; i++)
                {
                    string[] strArray = saveData.m_abnormaltyDataArray[i].Split(',');
                    SefiraEnum sefila = (SefiraEnum)int.Parse(strArray[0]);
                    int level = int.Parse(strArray[1]);
                    List<long> creatureList = new List<long>();

                    for (int j = 2; j < strArray.Length; j++)
                    {
                        creatureList.Add(long.Parse(strArray[j]));
                    }

                    //登録がない場合は0を設定
                    if (creatureList.Count == 0)
                    {
                        creatureList.Add(0);
                    }

                    settingData.orderCreature.SetOrder(new KeyValuePair<SefiraEnum, int>(sefila, level), creatureList);
                }

            }
            return settingData;
        }


        /// <summary>
        /// セッティングを保存
        /// </summary>
        /// <param name="enableAddLob"></param>
        /// <param name="_orderCreature"></param>
        public void SaveSettings(SettingData settingData)
        {
            EditSettingSaveData data = new EditSettingSaveData();
            data.m_varsion = EditSettingSaveData.m_saveDataVersion;
            data.m_addMob = settingData.enableAddLob;
            data.m_addEnergy = settingData.enableAddEnergy;
            data.m_alwayGetGift = settingData.enableAlwayGetGift;
            data.m_malkutNote = settingData.enableMalkutNote;
            data.m_taskManager = settingData.enableTaskManager;
            data.m_abnormaltyDataArray = new string[settingData.orderCreature.m_creatureOlderDic.Count];


            int count = 0;
            foreach (var kv in settingData.orderCreature.m_creatureOlderDic)
            {
                string abnormalty = "" + (int)(kv.Key.Key) + ","+ kv.Key.Value+",";

                for (int i = 0; i < kv.Value.Count; i++)
                {
                    if (1 <= i) abnormalty += ",";
                    abnormalty += kv.Value[i];
                }
                data.m_abnormaltyDataArray[count] = abnormalty;
                count++;
            }
            var json = JsonUtility.ToJson(data, true);
            string filePath = Application.persistentDataPath + "/" + EditSettingSaveData.m_saveDataName;

            SaveSettingText(filePath, json);
        }

        /// <summary>
        /// テキストファイル読み込み
        /// </summary>
        /// <returns></returns>
        private string LoadSettingText(string path)
        {
            //ストリームリーダーsrに読み込む
            string result = "";
            exceptionMessage = "";
            try
            {
                //※Application.dataPathはプロジェクトデータのAssetフォルダまでのアクセスパスのこと,
                using (StreamReader sr = new StreamReader(path))
                {
                    //ストリームリーダーをstringに変換
                    result = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch (Exception e)
            {
                exceptionMessage = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 設定データをテキストに保存
        /// </summary>
        /// <param name="path"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool SaveSettingText(string path,string text)
        {
            //ストリームライターwriterに書き込む
            try
            {
                using (StreamWriter writer = new StreamWriter(path, false))
                {
                    writer.Write(text);
                    writer.Flush();
                    writer.Close();
                }
            }
            catch (Exception e)
            {
                exceptionMessage = e.Message;
                return false;
            }
            return true;
        }
    }
}
