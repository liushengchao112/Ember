using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Sprites
{
    internal class DefaultPackerPolicySample : IPackerPolicy
    {
        protected class Entry
        {
            public Sprite sprite;

            public AtlasSettings settings;

            public string atlasName;

            public SpritePackingMode packingMode;

            public int anisoLevel;
        }

        private const uint kDefaultPaddingPower = 3u;
        private const int atlasMaxWidth = 1024;
        private const int atlasMaxHeight = 1024;

        protected virtual string TagPrefix
        {
            get
            {
                return "[TIGHT]";
            }
        }

        protected virtual bool AllowTightWhenTagged
        {
            get
            {
                return true;
            }
        }

        protected virtual bool AllowRotationFlipping
        {
            get
            {
                return false;
            }
        }

        public virtual int GetVersion()
        {
            return 1;
        }

        public void OnGroupAtlases( BuildTarget target, PackerJob job, int[] textureImporterInstanceIDs )
        {
            List<Entry> list = new List<Entry>();

            string text = "";
            if ( target != BuildTarget.NoTarget )
            {
                text = target.ToString();
            }

            for ( int i = 0; i < textureImporterInstanceIDs.Length; i++ )
            {
                TextureFormat textureFormat;
                ColorSpace colorSpace;
                int compressionQuality;

                int instanceID = textureImporterInstanceIDs[i];

                TextureImporter textureImporter = EditorUtility.InstanceIDToObject( instanceID ) as TextureImporter;
                textureImporter.ReadTextureImportInstructions( target, out textureFormat, out colorSpace, out compressionQuality );

                TextureImporterSettings textureImporterSettings = new TextureImporterSettings();
                textureImporter.ReadTextureSettings( textureImporterSettings );

                bool flag = text != "" && this.HasPlatformEnabledAlphaSplittingForCompression( text, textureImporter );

                Sprite[] array = ( from x in AssetDatabase.LoadAllAssetRepresentationsAtPath( textureImporter.assetPath )
                                   select x as Sprite into x
                                   where x != null
                                   select x ).ToArray<Sprite>();
                Sprite[] array2 = array;

                for ( int j = 0; j < array2.Length; j++ )
                {
                    Sprite sprite = array2[j];

                    Entry entry = new Entry();

                    entry.sprite = sprite;
                    entry.settings.format = textureFormat;
                    entry.settings.colorSpace = colorSpace;
                    entry.settings.compressionQuality = compressionQuality;
                    entry.settings.filterMode = ( ( !Enum.IsDefined( typeof( FilterMode ), textureImporter.filterMode ) ) ? FilterMode.Bilinear : textureImporter.filterMode );
                    entry.settings.maxWidth = atlasMaxWidth;
                    entry.settings.maxHeight = atlasMaxHeight;
                    entry.settings.generateMipMaps = textureImporter.mipmapEnabled;
                    entry.settings.enableRotation = this.AllowRotationFlipping;
                    entry.settings.allowsAlphaSplitting = ( textureFormat == TextureFormat.ETC_RGB4 && flag );

                    if ( textureImporter.mipmapEnabled )
                    {
                        entry.settings.paddingPower = kDefaultPaddingPower;
                    }
                    else
                    {
                        entry.settings.paddingPower = ( uint )EditorSettings.spritePackerPaddingPower;
                    }

                    entry.atlasName = this.ParseAtlasName( textureImporter.spritePackingTag );
                    entry.packingMode = this.GetPackingMode( textureImporter.spritePackingTag, textureImporterSettings.spriteMeshType );
                    entry.anisoLevel = textureImporter.anisoLevel;

                    list.Add( entry );
                }

                Resources.UnloadAsset( textureImporter );
            }

            IEnumerable<IGrouping<string, Entry>> enumerable = from e in list
                                                               group e by e.atlasName;
            foreach ( IGrouping<string, Entry> current in enumerable )
            {
                int num2 = 0;
                IEnumerable<IGrouping<AtlasSettings, Entry>> enumerable2 = from t in current
                                                                           group t by t.settings;
                foreach ( IGrouping<AtlasSettings, Entry> current2 in enumerable2 )
                {
                    string text2 = current.Key;

                    if ( enumerable2.Count<IGrouping<AtlasSettings, Entry>>() > 1 )
                    {
                        text2 += string.Format( " (Group {0})", num2 );
                    }

                    AtlasSettings key = current2.Key;
                    key.anisoLevel = 1;

                    if ( key.generateMipMaps )
                    {
                        foreach ( Entry current3 in current2 )
                        {
                            if ( current3.anisoLevel > key.anisoLevel )
                            {
                                key.anisoLevel = current3.anisoLevel;
                            }
                        }
                    }

                    job.AddAtlas( text2, key );

                    foreach ( Entry current4 in current2 )
                    {
                        job.AssignToAtlas( text2, current4.sprite, current4.packingMode, SpritePackingRotation.None );
                    }

                    num2++;
                }
            }
        }

        protected bool HasPlatformEnabledAlphaSplittingForCompression( string targetName, TextureImporter ti )
        {
            TextureImporterPlatformSettings platformTextureSettings = ti.GetPlatformTextureSettings( targetName );
            return platformTextureSettings.overridden && platformTextureSettings.allowsAlphaSplitting;
        }

        protected bool IsTagPrefixed( string packingTag )
        {
            packingTag = packingTag.Trim();
            return packingTag.Length >= this.TagPrefix.Length && packingTag.Substring( 0, this.TagPrefix.Length ) == this.TagPrefix;
        }

        private string ParseAtlasName( string packingTag )
        {
            string text = packingTag.Trim();
            if ( this.IsTagPrefixed( text ) )
            {
                text = text.Substring( this.TagPrefix.Length ).Trim();
            }
            return ( text.Length != 0 ) ? text : "(unnamed)";
        }

        private SpritePackingMode GetPackingMode( string packingTag, SpriteMeshType meshType )
        {
            SpritePackingMode result;
            if ( meshType == SpriteMeshType.Tight && this.IsTagPrefixed( packingTag ) == this.AllowTightWhenTagged )
            {
                result = SpritePackingMode.Tight;
            }
            else
            {
                result = SpritePackingMode.Rectangle;
            }
            return result;
        }
    }
}