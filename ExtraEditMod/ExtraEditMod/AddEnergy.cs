using UnityEngine;
namespace ExtraEditMod
{
    public class AddEnergy
    {
        //Lボタン一度につき加算するLOBポイント
        private const int ADD_ENERGY = 1;

        //キーを押しっぱなしにしている判定
        private const int KEEP_KEY_PUSH_COUNT = 30;

        /// <summary>
        /// エネルギーを追加できるか？
        /// </summary>
        static bool m_enableAddEnergy = false;
        public static bool enableAddEnergy { get { return m_enableAddEnergy; } set { m_enableAddEnergy = value; } }

        /// <summary>
        /// LOBを追加できるか？
        /// </summary>
        public static bool isAddEnergy { get { return m_enableAddEnergy; } }

        //キーを押している数
        static int m_keyPushedCount = 0;

        //キーを押しているフラグ
        static bool m_isPushedFlag = false;

        //エネルギー更新
        public void Update()
        {
            if (isAddEnergy && ( Input.GetKeyDown(KeyCode.I) || KEEP_KEY_PUSH_COUNT < m_keyPushedCount))
            {
                m_isPushedFlag = true;
                EnergyModel.instance.AddEnergy(ADD_ENERGY);
            }
            if (Input.GetKeyUp(KeyCode.I))
            {
                m_keyPushedCount = 0;
                m_isPushedFlag = false;
            }
            if (m_isPushedFlag)
            {
                m_keyPushedCount++;
            }

        }
    }

}