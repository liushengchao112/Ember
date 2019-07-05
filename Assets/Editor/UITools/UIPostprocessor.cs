using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

using UI;
using System.Collections.Generic;

namespace UIEditor
{
    public class UIPostprocessor : AssetPostprocessor
    {

        private const string UI_FILE_PATH = "Assets/Art/UI/";
        private const string UI_LOAD_PREFAB_PATH = "Assets/Art/UI/UILoadPrefabs";
        private const string SHADER_LOAD_PATH = "Assets/Art/Shaders/";
        private const string CHARACTER_FILE_PATH = "Assets/Art/Character/";
        private const string PARTICLE_FILE_PATH = "Assets/Art/Particle/";
        //private const int MAX_TETURE_SIZE = 512;
        private int COMPRESSION_QUALITY { get { return (int)TextureCompressionQuality.Best; } }

        private enum AudioType
        {
            Mp3 = 0,
            Wav = 1,
        }

        void OnPostprocessTexture( Texture2D texture )
        {
            bool isUITexture = assetPath.StartsWith( UI_FILE_PATH );

            TextureImporter textureImporter = assetImporter as TextureImporter;

            // Not the UI path
            if ( !isUITexture )
            {
                if( textureImporter.textureType == TextureImporterType.Sprite)
                {
                    textureImporter.spritePackingTag = "";
                }
                return;
            }

            if( assetPath.Contains( "/Atlas/MainUI.png" ) )
            {
                return;
            }

            // If it's big textrue, don't set the alignment, manually manage it
            if ( assetPath.Contains( "/BigTexture/" ) )
            {
                //textureImporter.ClearPlatformTextureSettings( "Android" );
                //textureImporter.ClearPlatformTextureSettings( "iPhone" );
                //textureImporter.ClearPlatformTextureSettings( "Standalone" );
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.wrapMode = TextureWrapMode.Clamp;
                textureImporter.filterMode = FilterMode.Bilinear;
                textureImporter.mipmapEnabled = false;
                textureImporter.isReadable = false;
                textureImporter.anisoLevel = 1;
                textureImporter.spritePackingTag = "";
                return;
            }

            string atlasName;

            // If it's a multiple folder, take its parent as the tag name
            string curFile = assetPath.Replace( UI_FILE_PATH, "" );
            string[] strs = curFile.Split( '/' );
            if ( strs.Length >= 2 && !strs[1].Contains( "." ) )
            {
                atlasName = strs[1];
            }
            else
            {
                Debug.LogError( "You can't put it in this folder! \n" + assetPath );
                return;
            }

            // TODO : Used to control the atlas in the future
            switch ( atlasName )
            {
                case "":
                {

                }
                break;
                default:
                {

                }
                break;
            }

            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.wrapMode = TextureWrapMode.Clamp;
            textureImporter.filterMode = FilterMode.Bilinear;
            textureImporter.mipmapEnabled = false;
            textureImporter.isReadable = false;
            textureImporter.anisoLevel = 1;
            textureImporter.spritePackingTag = atlasName;

            TextureImporterPlatformSettings settingAndroid = textureImporter.GetPlatformTextureSettings( "Android" );
            settingAndroid.overridden = true;
            settingAndroid.textureCompression = TextureImporterCompression.CompressedHQ;
            settingAndroid.compressionQuality = COMPRESSION_QUALITY;

            TextureImporterPlatformSettings settingIos = textureImporter.GetPlatformTextureSettings( "iPhone" );
            settingIos.overridden = true;

            TextureImporterPlatformSettings settingPc = textureImporter.GetPlatformTextureSettings( "Standalone" );
            settingPc.overridden = true;
            settingPc.crunchedCompression = true;
            settingPc.compressionQuality = COMPRESSION_QUALITY;

            if ( textureImporter.DoesSourceTextureHaveAlpha() )
            {
                textureImporter.alphaSource = TextureImporterAlphaSource.FromInput;
                textureImporter.alphaIsTransparency = true;
                textureImporter.allowAlphaSplitting = true;

                settingAndroid.allowsAlphaSplitting = true;
                settingAndroid.format = TextureImporterFormat.RGBA32;

                settingIos.textureCompression = TextureImporterCompression.Uncompressed;
                settingIos.compressionQuality = COMPRESSION_QUALITY;
                if ( assetPath.Contains( "/Img_RGBA32/" ) )
                {
                    settingIos.format = TextureImporterFormat.RGBA32;
                }
                else if ( assetPath.Contains( "/Img_RGBA16/" ) )
                {
                    settingIos.format = TextureImporterFormat.RGBA32;
                    //ImageDither4444( texture );
                }
                else
                {
                    settingIos.format = TextureImporterFormat.RGBA32;
                }

                settingPc.format = TextureImporterFormat.DXT5Crunched;
            }
            else
            {
                textureImporter.alphaSource = TextureImporterAlphaSource.None;
                textureImporter.alphaIsTransparency = false;
                textureImporter.allowAlphaSplitting = false;

                settingAndroid.allowsAlphaSplitting = false;
                settingAndroid.format = TextureImporterFormat.RGB24;

                settingIos.textureCompression = TextureImporterCompression.Uncompressed;
                settingIos.compressionQuality = COMPRESSION_QUALITY;
                settingIos.format = TextureImporterFormat.RGB24;

                settingPc.format = TextureImporterFormat.DXT1Crunched;
            }

            //if ( texture.width > MAX_TETURE_SIZE || texture.height > MAX_TETURE_SIZE )
            //{
            //    Debug.Log( "Find a resource that is larger than 512, path : " + assetPath );
            //}
            //settingAndroid.maxTextureSize > MAX_TETURE_SIZE ? MAX_TETURE_SIZE : settingAndroid.maxTextureSize;
            //settingIos.maxTextureSize > MAX_TETURE_SIZE ? MAX_TETURE_SIZE : settingIos.maxTextureSize;
            //settingPc.maxTextureSize = settingPc.maxTextureSize > MAX_TETURE_SIZE ? MAX_TETURE_SIZE : settingPc.maxTextureSize;
            settingAndroid.maxTextureSize = 1024;
            settingIos.maxTextureSize = 1024;
            settingPc.maxTextureSize = 1024;
            

            textureImporter.SetPlatformTextureSettings( settingAndroid );
            textureImporter.SetPlatformTextureSettings( settingIos );
            textureImporter.SetPlatformTextureSettings( settingPc );
        }

        void ImageDither4444( Texture2D texture )
        {
            var texw = texture.width;
            var texh = texture.height;

            var pixels = texture.GetPixels();
            var offs = 0;

            var k1Per15 = 1.0f / 15.0f;
            var k1Per16 = 1.0f / 16.0f;
            var k3Per16 = 3.0f / 16.0f;
            var k5Per16 = 5.0f / 16.0f;
            var k7Per16 = 7.0f / 16.0f;

            for ( var y = 0; y < texh; y++ )
            {
                for ( var x = 0; x < texw; x++ )
                {
                    float a = pixels[offs].a;
                    float r = pixels[offs].r;
                    float g = pixels[offs].g;
                    float b = pixels[offs].b;

                    var a2 = Mathf.Clamp01( Mathf.Floor( a * 16 ) * k1Per15 );
                    var r2 = Mathf.Clamp01( Mathf.Floor( r * 16 ) * k1Per15 );
                    var g2 = Mathf.Clamp01( Mathf.Floor( g * 16 ) * k1Per15 );
                    var b2 = Mathf.Clamp01( Mathf.Floor( b * 16 ) * k1Per15 );

                    var ae = a - a2;
                    var re = r - r2;
                    var ge = g - g2;
                    var be = b - b2;

                    pixels[offs].a = a2;
                    pixels[offs].r = r2;
                    pixels[offs].g = g2;
                    pixels[offs].b = b2;

                    var n1 = offs + 1;   // (x+1,y)
                    var n2 = offs + texw - 1; // (x-1 , y+1)
                    var n3 = offs + texw;  // (x, y+1)
                    var n4 = offs + texw + 1; // (x+1 , y+1)

                    if ( x < texw - 1 )
                    {
                        pixels[n1].a += ae * k7Per16;
                        pixels[n1].r += re * k7Per16;
                        pixels[n1].g += ge * k7Per16;
                        pixels[n1].b += be * k7Per16;
                    }

                    if ( y < texh - 1 )
                    {
                        pixels[n3].a += ae * k5Per16;
                        pixels[n3].r += re * k5Per16;
                        pixels[n3].g += ge * k5Per16;
                        pixels[n3].b += be * k5Per16;

                        if ( x > 0 )
                        {
                            pixels[n2].a += ae * k3Per16;
                            pixels[n2].r += re * k3Per16;
                            pixels[n2].g += ge * k3Per16;
                            pixels[n2].b += be * k3Per16;
                        }

                        if ( x < texw - 1 )
                        {
                            pixels[n4].a += ae * k1Per16;
                            pixels[n4].r += re * k1Per16;
                            pixels[n4].g += ge * k1Per16;
                            pixels[n4].b += be * k1Per16;
                        }
                    }

                    offs++;
                }
            }

            texture.SetPixels( pixels );
            EditorUtility.CompressTexture( texture, TextureFormat.RGBA4444, TextureCompressionQuality.Best );
        }

        void ImageDither565( Texture2D texture )
        {
            var texw = texture.width;
            var texh = texture.height;

            var pixels = texture.GetPixels();
            var offs = 0;

            var k1Per31 = 1.0f / 31.0f;

            var k1Per32 = 1.0f / 32.0f;
            var k5Per32 = 5.0f / 32.0f;
            var k11Per32 = 11.0f / 32.0f;
            var k15Per32 = 15.0f / 32.0f;

            var k1Per63 = 1.0f / 63.0f;

            var k3Per64 = 3.0f / 64.0f;
            var k11Per64 = 11.0f / 64.0f;
            var k21Per64 = 21.0f / 64.0f;
            var k29Per64 = 29.0f / 64.0f;

            var k_r = 32; //R&B压缩到5位，所以取2的5次方
            var k_g = 64; //G压缩到6位，所以取2的6次方

            for ( var y = 0; y < texh; y++ )
            {
                for ( var x = 0; x < texw; x++ )
                {
                    float r = pixels[offs].r;
                    float g = pixels[offs].g;
                    float b = pixels[offs].b;

                    var r2 = Mathf.Clamp01( Mathf.Floor( r * k_r ) * k1Per31 );
                    var g2 = Mathf.Clamp01( Mathf.Floor( g * k_g ) * k1Per63 );
                    var b2 = Mathf.Clamp01( Mathf.Floor( b * k_r ) * k1Per31 );

                    var re = r - r2;
                    var ge = g - g2;
                    var be = b - b2;

                    var n1 = offs + 1;
                    var n2 = offs + texw - 1;
                    var n3 = offs + texw;
                    var n4 = offs + texw + 1;

                    if ( x < texw - 1 )
                    {
                        pixels[n1].r += re * k15Per32;
                        pixels[n1].g += ge * k29Per64;
                        pixels[n1].b += be * k15Per32;
                    }

                    if ( y < texh - 1 )
                    {
                        pixels[n3].r += re * k11Per32;
                        pixels[n3].g += ge * k21Per64;
                        pixels[n3].b += be * k11Per32;

                        if ( x > 0 )
                        {
                            pixels[n2].r += re * k5Per32;
                            pixels[n2].g += ge * k11Per64;
                            pixels[n2].b += be * k5Per32;
                        }

                        if ( x < texw - 1 )
                        {
                            pixels[n4].r += re * k1Per32;
                            pixels[n4].g += ge * k3Per64;
                            pixels[n4].b += be * k1Per32;
                        }
                    }

                    pixels[offs].r = r2;
                    pixels[offs].g = g2;
                    pixels[offs].b = b2;

                    offs++;
                }
            }

            texture.SetPixels( pixels );
            EditorUtility.CompressTexture( texture, TextureFormat.RGB565, TextureCompressionQuality.Best );
        }

        void OnPreprocessAudio()
        {
            if ( assetPath.Contains( "/Plugins/" ) )
                return;

            if ( assetPath.EndsWith( ".wav" )
                  || assetPath.EndsWith( ".WAV" )
                  || assetPath.EndsWith( ".mp3" )
                  || assetPath.EndsWith( ".MP3" ) )
            {
                SetAudioImporter( assetPath );
            }
            else
            {
                Debug.LogError( "This type of sound file is not supported, path : " + assetPath );
            }
        }

        static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath )
        {
            foreach ( string move in movedAssets )
            {
                // Move the folder to execute
                AssetDatabase.ImportAsset( move );
            }

            foreach ( string importe in importedAssets )
            {
                if ( importe.StartsWith( "Assets/Art/UI/LoadTexture/" ) && importe.Contains(".") )
                {
                    MakeSpritePrefab( importe, "LoadTexture" );
                }
                else if ( importe.StartsWith( "Assets/Art/UI/LastLoadTexture/" ) && importe.Contains( "." ) )
                {
                    MakeSpritePrefab( importe, "LastLoadTexture" );
                }
                else if ( importe.StartsWith( CHARACTER_FILE_PATH ) 
                    && importe.EndsWith( ".prefab" )
                    && importe.Contains( "/Prefabs/" ) 
                    && !importe.EndsWith( "_lod.prefab" ) )
                {
                    GameObject cPrefab = AssetDatabase.LoadAssetAtPath<GameObject>( importe );
                    Animator animator = cPrefab.GetComponent<Animator>();
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    EditorUtility.SetDirty( cPrefab );
                }
            }

            foreach ( string deleted in deletedAssets )
            {
                if ( (deleted.StartsWith( "Assets/Art/UI/LoadTexture/" ) || deleted.StartsWith( "Assets/Art/UI/LastLoadTexture/" )) && deleted.Contains( "." ) )
                {
                    DelSpritePrefab( deleted );
                }
            }
        }

        void OnPostprocessModel( GameObject g )
        {
            if ( !assetPath.Contains( "/Art/Character/" ) ) return;

            Debug.Log( "import model : " + assetPath );

            //Character
            if ( assetPath.StartsWith( CHARACTER_FILE_PATH ) )
            {
                ModelImporter modelImporter = assetImporter as ModelImporter;
                modelImporter.meshCompression = ModelImporterMeshCompression.Off;
                modelImporter.isReadable = false;
                modelImporter.optimizeMesh = true;
                modelImporter.importBlendShapes = false;
                modelImporter.weldVertices = true;
                modelImporter.importNormals = ModelImporterNormals.Import;
                modelImporter.importMaterials = false;
                modelImporter.animationType = ModelImporterAnimationType.Generic;
                //Debug.Log( modelImporter.sourceAvatar.name );

                if ( assetPath.Contains( "/Model/" ) )
                {
                    modelImporter.importAnimation = false;
                }
                else
                {
                    modelImporter.importAnimation = true;
                }

                if ( assetPath.Contains( "_lod" ) )
                {
                    modelImporter.importTangents = ModelImporterTangents.None;
                }
                else
                {
                    modelImporter.importTangents = ModelImporterTangents.CalculateMikk;
                }
            }

            //Particle
            if ( assetPath.StartsWith( PARTICLE_FILE_PATH ) )
            {
                if( assetPath.StartsWith( PARTICLE_FILE_PATH + "Character/" ) )
                {

                }
            }
        }

        void OnPostprocessGameObjectWithUserProperties( GameObject go, string[] propNames, System.Object[] values )
        {

        }

        private void SetAudioImporter( string path )
        {

            AudioImporter audio = AssetImporter.GetAtPath( path ) as AudioImporter;

            if ( audio == null )
            {
                Debug.LogError( "not found audio path:" + path );
                return;
            }
            audio.forceToMono = false;
            audio.loadInBackground = false;
            audio.preloadAudioData = false;

            AudioImporterSampleSettings settingAndroid = new AudioImporterSampleSettings();
            AudioImporterSampleSettings settingIphone = new AudioImporterSampleSettings();
            AudioImporterSampleSettings settingStandalone = new AudioImporterSampleSettings();

            if ( path.Contains( "/BGM/" ) )
            {
                settingAndroid.loadType = AudioClipLoadType.Streaming;
                settingIphone.loadType = AudioClipLoadType.Streaming;
                settingStandalone.loadType = AudioClipLoadType.Streaming;
            }
            else if ( path.Contains( "/Sounds/" ) || path.Contains( "/Common/" ) || path.Contains( "/MainMenu/" ) || path.Contains( "/Battle/" ) )
            {
                settingAndroid.loadType = AudioClipLoadType.DecompressOnLoad;
                settingIphone.loadType = AudioClipLoadType.DecompressOnLoad;
                settingStandalone.loadType = AudioClipLoadType.DecompressOnLoad;
            }
            else
            {
                Debug.LogError( "You can't put it in this folder! \n" + path );
                return;
            }


            if ( path.Contains( "/High/" ) )
            {
                settingAndroid.compressionFormat = AudioCompressionFormat.Vorbis;
                settingAndroid.quality = 1f;
                settingAndroid.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;

                settingIphone.compressionFormat = AudioCompressionFormat.Vorbis;
                settingIphone.quality = 1f;
                settingIphone.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;

                settingStandalone.compressionFormat = AudioCompressionFormat.Vorbis;
                settingStandalone.quality = 1f;
                settingStandalone.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            }
            else if ( path.Contains( "/Middle/" ) )
            {
                settingAndroid.compressionFormat = AudioCompressionFormat.Vorbis;
                settingAndroid.quality = 0.5f;
                settingAndroid.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;

                settingIphone.compressionFormat = AudioCompressionFormat.Vorbis;
                settingIphone.quality = 0.5f;
                settingIphone.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;

                settingStandalone.compressionFormat = AudioCompressionFormat.Vorbis;
                settingStandalone.quality = 0.5f;
                settingStandalone.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
            }
            else
            {
                settingAndroid.compressionFormat = AudioCompressionFormat.Vorbis;
                settingAndroid.quality = 0.01f;
                settingAndroid.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
                settingAndroid.sampleRateOverride = 11025;

                settingIphone.compressionFormat = AudioCompressionFormat.Vorbis;
                settingIphone.quality = 0.01f;
                settingIphone.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
                settingIphone.sampleRateOverride = 11025;

                settingStandalone.compressionFormat = AudioCompressionFormat.Vorbis;
                settingStandalone.quality = 0.01f;
                settingStandalone.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
                settingStandalone.sampleRateOverride = 11025;
            }

            audio.SetOverrideSampleSettings( "Android", settingAndroid );
            audio.SetOverrideSampleSettings( "iPhone", settingIphone );
            audio.SetOverrideSampleSettings( "Standalone", settingStandalone );

        }

        static void MakeSpritePrefab( string path, string parentDir )
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>( path );
            if( sprite == null )
            {
                Debug.Log( "sprite is null, path:" + path );
                return;
            }

            string targetPath = path.Replace( UI_FILE_PATH + parentDir, UI_LOAD_PREFAB_PATH );
            FileInfo fileInfo = new FileInfo( targetPath );
            string targetDir = fileInfo.Directory + "/";
            if ( !Directory.Exists( targetDir ) )
            {
                Directory.CreateDirectory( targetDir );
            }

            string createPath = targetPath.Replace( fileInfo.Extension, ".prefab" );
            if ( !File.Exists( createPath ) )
            {
                GameObject go = new GameObject( sprite.name );
                AtlasSprite atlasSprite = go.AddComponent<AtlasSprite>();
                atlasSprite.sprite = sprite;
                PrefabUtility.CreatePrefab( createPath, go );
                GameObject.DestroyImmediate( go );

                Debug.Log( "Create Prefab : " + createPath );
            }
        }

        static void DelSpritePrefab( string path )
        {
            string targetPath = path.Replace( UI_FILE_PATH + "LoadTexture", UI_LOAD_PREFAB_PATH );
            FileInfo fileInfo = new FileInfo( targetPath );
            string delPath = targetPath.Replace( fileInfo.Extension, ".prefab" );
            if ( File.Exists( delPath ) )
            {
                AssetDatabase.DeleteAsset( delPath );
                Debug.Log( "Delete ui load prefab :" + delPath );
            }
        }

    }
}