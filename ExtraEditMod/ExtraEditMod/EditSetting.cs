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
        /// <summary>
        /// 例外時のメッセージ
        /// </summary>
        string exceptionMessage { get; set; }


        /// <summary>
        /// セッティングを読み込む
        /// </summary>
        /// <param name="enableAddLob"></param>
        /// <param name="_orderCreature"></param>
        public void LoadSettings(out bool enableAddLob , out bool enableAddEnergy, out bool enableAlwayGetGift, OrderCreature _orderCreature)
        {
            enableAddLob = true;
            enableAddEnergy = false;
            enableAlwayGetGift = false;

            string filePath = Application.persistentDataPath + "/" + EditSettingSaveData.m_saveDataName;
            string json = LoadSettingText(filePath);
            EditSettingSaveData saveData = JsonUtility.FromJson<EditSettingSaveData>(json);
            if (saveData != null)
            {
                enableAddLob = saveData.m_addMob;
                enableAddEnergy = saveData.m_addEnergy;
                enableAlwayGetGift = saveData.m_alwayGetGift;

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
    
                    _orderCreature.SetOrder(new KeyValuePair<SefiraEnum, int>(sefila, level), creatureList);
                }

            }
        }


        /// <summary>
        /// セッティングを保存
        /// </summary>
        /// <param name="enableAddLob"></param>
        /// <param name="_orderCreature"></param>
        public void SaveSettings(bool enableAddLob,bool enableAddEnergy ,bool enableAlwaysGetGift,OrderCreature _orderCreature)
        {
            EditSettingSaveData data = new EditSettingSaveData();
            data.m_varsion = EditSettingSaveData.m_saveDataVersion;
            data.m_addMob = enableAddLob;
            data.m_addEnergy = enableAddEnergy;
            data.m_alwayGetGift = enableAlwaysGetGift;

            data.m_abnormaltyDataArray = new string[_orderCreature.m_creatureOlderDic.Count];


            int count = 0;
            foreach (var kv in _orderCreature.m_creatureOlderDic)
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
