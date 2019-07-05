using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AtlasSprite : MonoBehaviour
    {
        public Sprite sprite;
        public Sprite AlphaSprite;
    }

    public static class ImageExpand
    {
        private static Material grayMaterial;
        private static Material highLightMaterial;
        /// <summary>
        /// Gray
        /// </summary>
        public static Material GrayMaterial
        {
            get
            {
                if ( grayMaterial == null )
                {
                    //TODO : RGBA32 branch special modify
                    //#if UNITY_ANDROID
                    //                    grayMaterial = new Material( Shader.Find( "UI/UIGrayAndroid" ) );
                    //#else
                    //                    grayMaterial = new Material( Shader.Find( "UI/UIGray" ) );
                    //#endif
                    grayMaterial = new Material( Shader.Find( "UI/UIGray" ) );
                }
                return grayMaterial;
            }
        }

        /// <summary>
        /// HighLight
        /// </summary>
        public static Material HighLightMaterial
        {
            get
            {
                if ( highLightMaterial == null )
                {
                    highLightMaterial = new Material( Shader.Find( "UI/UIHighlight" ) );
                }

                highLightMaterial.SetFloat( "_SpecPower", 1.6f );
                return highLightMaterial;
            }
        }

        /// <summary>
        /// my Expand : set sprite
        /// </summary>
        /// <param name="atlasSprite">load AtlasSprite</param>
        public static void SetSprite( this Image img, AtlasSprite atlasSprite )
        {
            if( atlasSprite != null )
            {
                img.sprite = atlasSprite.sprite;
            }
            else
            {
                img.sprite = null;
            }
        }

        /// <summary>
        /// my Expand : set Gray sprite
        /// </summary>
        /// <param name="img"></param>
        /// <param name="show"></param>
        public static void SetGray( this Image img, bool show )
        {
            img.material = show ? GrayMaterial : null;
        }

        /// <summary>
        /// Set HighLight Sprite
        /// </summary>
        /// <param name="img"></param>
        /// <param name="show"></param>
        public static void SetHighLight( this Image img, bool show )
        {
            img.material = show ? HighLightMaterial : null;
        }

        /// <summary>
        /// my Expand : set alpha sprite
        /// </summary>
        /// <param name="atlasSprite">load AtlasSprite</param>
        public static void SetAlphaSprite( this Image img, AtlasSprite atlasSprite )
        {
            img.overrideSprite = atlasSprite.sprite;

            // TODO : future
            img.material = null;
        }

    }
}