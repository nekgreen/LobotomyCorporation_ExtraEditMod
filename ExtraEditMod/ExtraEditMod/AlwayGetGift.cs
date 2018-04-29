using UnityEngine;
namespace ExtraEditMod
{
    /// <summary>
    /// 必ずギフトを手に入れられる
    /// </summary>
    public class AlwayGetGift
    {
        /// <summary>
        /// 必ずギフトが手に入るか？
        /// </summary>
        static bool m_enableAlwayGetGift = false;
        public static bool enableAlwayGetGift { get { return m_enableAlwayGetGift; } set { m_enableAlwayGetGift = value; } }

        /// <summary>
        /// ランダムの値か、0を受け取る
        /// </summary>
        /// <returns></returns>
        public static float GetRamdomeValueOrZero()
        {
            if (m_enableAlwayGetGift)
            {
                return 0.0f;
            }
            return UnityEngine.Random.value;
        }
    }
}