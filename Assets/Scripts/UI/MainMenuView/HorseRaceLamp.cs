using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class HorseRaceLamp : MonoBehaviour
    {
        private Text txt_racelamp;
        private RectTransform tra_racelamptransform; 

        private bool isshow = false;
        private int maskwidth;
        private int txtwidth=-1;
        private int lampcount=0;

        [SerializeField]
        private float speed = 100;
        private LampMessage lp;
        private Vector3 txt_racelamp_localpostion;

        public float totalmovex = 0;
        private HorseRaceLampType currentType= HorseRaceLampType.None;

        void Awake()
        {            
            txt_racelamp = this.transform.Find("NoticeText").GetComponent<Text>();
            tra_racelamptransform = this.GetComponent<RectTransform>();
            txt_racelamp_localpostion = txt_racelamp.transform.localPosition;
            maskwidth = (int)tra_racelamptransform.rect.width;
        }

        void Update()
        {
            if (isshow)
            {
                float movespeed = speed * Time.deltaTime;
                txt_racelamp.transform.localPosition -= new Vector3(movespeed, 0, 0);
                totalmovex += movespeed;
                if (txtwidth<=0)
                {
                    txtwidth = (int)txt_racelamp.rectTransform.rect.width;
                }
                if (totalmovex >= (txtwidth + maskwidth))
                {
                    lampcount += 1;
                    if (lampcount == lp.count)
                    {
                        isshow = false;
                        currentType = HorseRaceLampType.None;
                    }
                    else
                    {
                        txt_racelamp.transform.localPosition = txt_racelamp_localpostion;
                        totalmovex = 0;
                    }
                }
            }
        }

        public void Init(LampMessage lamp)
        {
            lp = lamp;
            txt_racelamptext = lp.text;
            currentType = lamp.type;
            txt_racelamp.transform.localPosition = txt_racelamp_localpostion;
            totalmovex = 0;
        }

        public HorseRaceLampType GetCurretType()
        {
            return currentType;
        }

        private string  txt_racelamptext
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    txt_racelamp.text = value;
                    txtwidth = -1;
                    lampcount = 0;
                    isshow = true;                   
                }                
            }
        }
    }

    public struct LampMessage
    {
        public HorseRaceLampType type;
        public string text;
        public int count;
    }

    public enum HorseRaceLampType
    {
        None=0,
        ApplictionMessage,
        PlayerMessage
    }
}