using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtraEditMod
{
    /// <summary>
    /// セーブデータ
    /// </summary>
    class EditSettingSaveData
    {
        /// <summary>
        /// セーブデータのバージョン
        /// </summary>
        public const string m_saveDataVersion = "0.0.2";

        /// <summary>
        /// セーブデーターの保存名
        /// </summary>
        public const string m_saveDataName = "EditSettingSaveData.txt";

        /// <summary>
        /// バージョン番号
        /// </summary>
        public string m_varsion;

        /// <summary>
        /// LOBを追加できるか？
        /// </summary>
        public bool m_addMob = true;

        /// <summary>
        /// エネルギーを追加できるか？
        /// </summary>
        public bool m_addEnergy = false;

        /// <summary>
        /// アブノーマリティのデータ配列
        /// </summary>
        public string[] m_abnormaltyDataArray;
    }
}