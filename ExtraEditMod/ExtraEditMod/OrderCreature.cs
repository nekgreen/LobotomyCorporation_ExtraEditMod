using System;
using System.Collections.Generic;
using CreatureGenerate;

namespace ExtraEditMod
{
    public class OrderCreature
    {
        public static int DAY_COUNT = 35;

        /// <summary>
        /// クリーチャーを選択できるか?
        /// </summary>
        static bool m_isSelectCreature = true;
        public static bool selectCreature { get { return m_isSelectCreature; } set { m_isSelectCreature = value; } }

        /// <summary>
        /// クリーチャーを選択できるか?
        /// </summary>
        public static bool isSelectCreature { get { return m_isSelectCreature; } }

        /// <summary>
        /// クリーチャーのオーダー
        /// </summary>
        public Dictionary<KeyValuePair<SefiraEnum, int>, List<long>> m_creatureOlderDic = new Dictionary<KeyValuePair<SefiraEnum, int>, List<long>>();

        /// <summary>
        /// 日付からセフィラを取得
        /// </summary>
        public Dictionary<int, SefiraEnum> m_dayToSefiraEnumDic = new Dictionary<int, SefiraEnum>();


        /// <summary>
        /// 選択できるアブノーマリティの候補
        /// </summary>
        /// <returns></returns>
        public List<long> GetCreatureList()
        {
            var list = new List<long>();
            list.Add(0);
            list.AddRange(CreatureGenerateInfo.all);

            //既に選択されてるやつは候補から取り除く
            foreach (var kv in m_creatureOlderDic)
            {
                if (kv.Value != null && 0 < kv.Value.Count)
                {
                    foreach (var id in kv.Value)
                    {
                        list.Remove(id);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 現在選択しているオーダーを非表示へ
        /// </summary>
        public void Init()
        {
            SefiraEnum se = SefiraEnum.MALKUT;
            for (int i = 0; i < (int)(SefiraEnum.DAAT) * 5; i++)
            {
                if (i == 22 || i == 23)
                {
                    se = SefiraEnum.TIPERERTH2;
                }
                if (0 < i && i % 5 == 0)
                {
                    se++;
                }
                m_dayToSefiraEnumDic.Add(i, se);
            }

            for (int i = (int)(SefiraEnum.MALKUT); i < (int)(SefiraEnum.DAAT); i++)
            {
                for (int level = 1; level < 5; level++)
                {
                    KeyValuePair<SefiraEnum, int> kv = new KeyValuePair<SefiraEnum, int>((SefiraEnum)i, level);
                    var list = new List<long>();
                    list.Add(0);
                    if (kv.Key == SefiraEnum.TIPERERTH1 || kv.Key == SefiraEnum.TIPERERTH2)
                    {
                        list.Add(0);
                    }
                    m_creatureOlderDic.Add(kv, list);
                }
            }
        }

        /// <summary>
        /// オーダーをセットする
        /// </summary>
        /// <param name="kv"></param>
        /// <param name="order"></param>
        public void SetOrder(KeyValuePair<SefiraEnum, int> kv, List<long> order)
        {
            m_creatureOlderDic[kv] = new List<long>(order);
        }

        /// <summary>
        /// ランダムにセットする
        /// </summary>
        /// <param name="kv"></param>
        public void SetRandomFromBaseKv(KeyValuePair<SefiraEnum, int> baseKv, int index)
        {
            SefiraEnum baseSefiraEnum = baseKv.Key;
            //減殺設定されている一からそのセフィラのレベル分までランダムをぶっこむ
            for (int level = baseKv.Value; level < 5; level++)
            {
                SetRandom(baseSefiraEnum, level, index);
                index = 0;

            }

            //その後のセフィラに関してもランダムをぶち込む。
            for (int i = (int)(baseSefiraEnum) + 1; i < (int)(SefiraEnum.DAAT); i++)
            {
                for (int level = 1; level < 5; level++)
                {
                    SetRandom((SefiraEnum)i, level);
                }
            }
        }

        /// <summary>
        /// ランダムを入れていく
        /// </summary>
        /// <param name="sefira"></param>
        /// <param name="level"></param>
        /// <param name="index"></param>
        void SetRandom(SefiraEnum sefira, int level, int index = 0)
        {
            KeyValuePair<SefiraEnum, int> kv = new KeyValuePair<SefiraEnum, int>(sefira, level);
            if (m_creatureOlderDic.ContainsKey(kv))
            {
                for (int i = index; i < m_creatureOlderDic[kv].Count; i++)
                {
                    m_creatureOlderDic[kv][i] = 0;
                }
            }
        }


        /// <summary>
        /// 選択のない日
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public bool isNoChoiceDay(int day)
        {
            return (day + 1) % 5 == 0;
        }

        /// <summary>
        /// クリーチャーをセットする
        /// </summary>
        /// <param name="day"></param>
        /// <param name="cgm"></param>
        public void SetCreature(object obj)
        {
            CreatureGenerateModel cgm = obj as CreatureGenerateModel;
            SetCreature(cgm.day, cgm);
        }

        /// <summary>
        /// クリーチャーをセットする
        /// </summary>
        /// <param name="day"></param>
        /// <param name="cgm"></param>
        public void SetCreature(int day, CreatureGenerateModel cgm)
        {
            ExtraEditMod.m_debuglog = "SetCreature";

            if (day < 0)
            {
                ExtraEditMod.m_debuglog += "\n use Default";
                SetCreatureDefault(day, cgm);
                return;
            }
            
            var sefiraEnum = m_dayToSefiraEnumDic[day];
            KeyValuePair<SefiraEnum, int> kv = new KeyValuePair<SefiraEnum, int>(sefiraEnum, day%5+1);
            if (m_creatureOlderDic.ContainsKey(kv))
            {
                
                bool result = false;
                foreach (var id in m_creatureOlderDic[kv])
                {
                    if (id != 0)
                    {
                        cgm.creature.Add(id);
                        ExtraEditMod.m_debuglog += "\n id"+ id;
                        result = true;
                    }
                }
                if (result) return;
            }
            ExtraEditMod.m_debuglog += "\n use Default";
            SetCreatureDefault(day, cgm);
        }

        /// <summary>
        /// クリーチャーのデフォルトをセット
        /// </summary>
        /// <param name="day"></param>
        /// <param name="cgm"></param>
        public void SetCreatureDefault(int day, CreatureGenerateModel cgm)
        {
            if (cgm.commonAction != null)
            {
                cgm.commonAction.Exectue();
            }
            if (cgm.stop)
            {
                return;
            }
            if (cgm.door1.commonAction != null)
            {
                cgm.door1.commonAction.Exectue();
            }
            if (cgm.door2.commonAction != null)
            {
                cgm.door2.commonAction.Exectue();
            }
            if (cgm.door3.commonAction != null)
            {
                cgm.door3.commonAction.Exectue();
            }
            cgm.door1.SetCreature();
            cgm.door2.SetCreature();
            cgm.door3.SetCreature();
            if (cgm.door1.Creature != (long)-1)
            {
                cgm.creature.Add(cgm.door1.Creature);
            }
            if (cgm.door2.Creature != (long)-1)
            {
                cgm.creature.Add(cgm.door2.Creature);
            }
            if (cgm.door3.Creature != (long)-1)
            {
                cgm.creature.Add(cgm.door3.Creature);
            }
        }
    }
}
