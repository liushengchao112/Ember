using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

using Utils;
using Constants;
using Data;
using Network;
using Resource;

namespace UI
{
    public class LoadingSceneView : MonoBehaviour
    {

        private enum LoadStep
        {
            Prepare = 0,

            /// <summary>
            /// first, load Battle 30%
            /// </summary>
            BeginBaseScene,

            LoadingBaseScene,

            /// <summary>
            /// second, load real battle scene 70%
            /// </summary>
            BeginBattleScene,

            LoadingBattleScene,

            BeginBattleMap,

            LoadingBattleMap,

            Done
        }

        private enum MyBattleType
        {
            Pvp = 0,
            Pve = 1,
			Tutorial = 2,
        }

        private MatchLoadItem match2V2LoadItem;
        private Transform tfFriendSideGroup;
        private Transform tfEnemySideGroup;
        private List<MatchLoadItem> cacheList;
        private AsyncOperation async;
        private ResourceRequest resourceRequest;
        private int updataRunNum;
        private LoadStep loadStep;
        private string battleSceneName;
        private string battleMapName;
        private List<MatchLoadItemVo> dataList;
        private MyBattleType myBattleType;
        private bool simulateBattle;
		private BattleType battleType;

        void Awake()
        {
            NetworkManager.RegisterServerMessageHandler( MsgCode.NoticeMessage, OnNoticeRespond );
        }

        void OnDestroy()
        {
            NetworkManager.RemoveServerMessageHandler( MsgCode.NoticeMessage, OnNoticeRespond );
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            if (cacheList != null)
            {
                cacheList.Clear();
            }
            cacheList = null;
            if (dataList != null)
            {
                dataList.Clear();
            }
            dataList = null;
        }

        void Start()
        {
            DontDestroyOnLoad( gameObject );

            InitData();
            InitView();

            loadStep = LoadStep.BeginBattleScene;
        }

        private void InitData()
        {
            ViewBase.ClearViewStack();
            DataManager client = DataManager.GetInstance();
            UIManager.locateState = UIManagerLocateState.Battle;
            client.SetPlayerIsInBattle( true );
            List<Matcher> list = client.GetMatchers();
            battleType = client.GetBattleType();

            simulateBattle = client.GetBattleSimluateState();

            if ( battleType == BattleType.BattleP1vsP1 )
            {
                battleSceneName = "Battle_1v1";
                battleMapName = "Prefabs/Map/1V1/BattleMapNew";
                myBattleType = MyBattleType.Pvp;
            }
			else if( battleType == BattleType.Tranining )
			{
				battleSceneName = "Battle_PvE";
				battleMapName = "Prefabs/Map/PVE/BattleMap";
				myBattleType = MyBattleType.Pve;
			}
			else if( battleType == BattleType.Survival )
			{
				battleSceneName = "Battle_PvE";
				battleMapName = "Prefabs/Map/1V1/BattleMapOld";
				myBattleType = MyBattleType.Pve;
			}
			else if( battleType == BattleType.BattleP2vsP2 )
			{
				battleSceneName = "Battle_2v2";
				battleMapName = "Prefabs/Map/2V2/BattleMap";
				myBattleType = MyBattleType.Pvp;
			}
			else if( battleType == BattleType.Tutorial )
			{
				battleSceneName = "Battle_Tutorial";
				battleMapName = "Prefabs/Map/1V1/BattleMapNew";
				myBattleType = MyBattleType.Tutorial;
			}

            dataList = new List<MatchLoadItemVo>();
            int length = list == null ? 0 : list.Count;

            for (int i = 0; i < length; i++)
            {
                MatchLoadItemVo data = new MatchLoadItemVo();
                data.playerID = list[i].playerId;
                data.playerName = list[i].name;
                data.playerIcon = list[i].portrait;
                data.sideIndex = list[i].side;
                dataList.Add( data );
            }
        }

        private void InitView()
        {
            GameObject curView = GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( "FightLoadingScreen" );
            RectTransform viewTf = GameObject.Instantiate( curView ).GetComponent<RectTransform>();
            viewTf.SetParent( transform );
            viewTf.localScale = Vector3.one;
            viewTf.localPosition = Vector3.zero;
            viewTf.localRotation = Quaternion.identity;
            viewTf.sizeDelta = Vector2.zero;
            tfFriendSideGroup = viewTf.Find( "FriendSideGroup" );
            tfEnemySideGroup = viewTf.Find( "EnemySideGroup" );

            GameObject go = GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( "MatchLoadItem" );
            match2V2LoadItem = GameObject.Instantiate( go ).AddComponent<MatchLoadItem>();
            match2V2LoadItem.transform.localScale = Vector3.one;
            match2V2LoadItem.transform.localPosition = Vector3.zero;
            match2V2LoadItem.transform.localRotation = Quaternion.identity;
            ShowList( dataList );

            updataRunNum = 0;
        }

        void Update()
        {
            switch ( loadStep )
            {
                case LoadStep.Prepare:
                case LoadStep.Done:
                    break;
                // Save loading base scene code
                //case LoadStep.BeginBaseScene:
                //{
                //    StartCoroutine( LoadSceneAsync( "Battle", LoadSceneMode.Single ) );
                //    DebugUtils.Log( DebugUtils.Type.LoadingScene, "begin to load base data!" );
                //    loadStep = LoadStep.LoadingBaseScene;
                //    break;
                //}
                case LoadStep.BeginBattleScene:
                {
                    StartCoroutine( LoadSceneAsync( battleSceneName, LoadSceneMode.Single ) );
                    DebugUtils.Log( DebugUtils.Type.LoadingScene, "begin to load real battle scene!" );
                    loadStep = LoadStep.LoadingBattleScene;
                    break;
                }
                case LoadStep.BeginBattleMap:
                {
                    StartCoroutine( LoadMapAsync( battleMapName ) );
                    DebugUtils.Log( DebugUtils.Type.LoadingScene, "begin to load battle map!" );
                    loadStep = LoadStep.LoadingBattleMap;
                    break;
                }
                case LoadStep.LoadingBaseScene:
                case LoadStep.LoadingBattleScene:
                case LoadStep.LoadingBattleMap:
                {
                    UpdateProgress();
                    break;
                }
                default:
                {
                    DebugUtils.LogError( DebugUtils.Type.LoadingScene, "There isn't such load step : " + loadStep );
                    break;
                }
            }
        }

        private void UpdateProgress()
        {
            updataRunNum++;
            if ( updataRunNum % 60 == 0 )
            {
                if (myBattleType == MyBattleType.Pve || simulateBattle )
                {
                    UpdataProgressOther( DataManager.GetInstance().GetPlayerId(), 100 );
                }
                else if (myBattleType == MyBattleType.Pvp)
                {
                    int progress = LoadingProgress();
                    DebugUtils.Log( DebugUtils.Type.LoadingScene, string.Format( "the loading progress is {0}, the load step is {1}, loading frame is {2}.", progress, loadStep, updataRunNum ) );
                    SendProgress( NoticeType.BattleLoading, progress );
                }
            }
        }

        private IEnumerator LoadSceneAsync(string name, LoadSceneMode mode)
        {
            async = SceneManager.LoadSceneAsync( name, mode );
            yield return async;

            //if( loadStep == LoadStep.LoadingBaseScene )
            //{
            //    loadStep = LoadStep.BeginBattleScene;
            //}
            //else if( loadStep == LoadStep.LoadingBattleScene )

            if ( loadStep == LoadStep.LoadingBattleScene )
            {
                //loadStep = LoadStep.BeginBattleMap;
                LoadingDone();
            }
            else
            {
                DebugUtils.Assert( false, string.Format( "the load step is {0} after loading scene asynchronously!", loadStep ) );
            }
        }

        private IEnumerator LoadMapAsync( string name )
        {
            resourceRequest = Resources.LoadAsync<Object>( name );
            yield return resourceRequest;

            LoadingDone( resourceRequest.asset );
        }

        private void LoadingDone( Object asset = null )
        {
            loadStep = LoadStep.Done;
            DebugUtils.Log( DebugUtils.Type.LoadingScene, "the battle map loading has been done!" );
            MessageDispatcher.PostMessage( MessageType.LoadBattleComplete, asset );
            UpdataProgressByID( DataManager.GetInstance().GetPlayerId(), 100, NoticeType.EnterBattle );

            if ( simulateBattle )
            {
                UpdataProgressOther( DataManager.GetInstance().GetPlayerId(), 100 );
                Invoke( "OnEnterSence", 2 );
            }
            else
            {
                if ( myBattleType == MyBattleType.Pvp )
                {
                    SendProgress( NoticeType.BattleLoading, 100 );
                    SendProgress( NoticeType.EnterBattle, 100 );
                }
                else if ( myBattleType == MyBattleType.Pve )
                {
                    UpdataProgressOther( DataManager.GetInstance().GetPlayerId(), 100 );
                    Invoke( "OnEnterSence", 2 );
                }
				else if( myBattleType == MyBattleType.Tutorial )
				{
					UpdataProgressOther( DataManager.GetInstance().GetPlayerId(), 100 );
					Invoke( "OnEnterSence", 2 );
				}
            }
        }

        private void ShowList(List<MatchLoadItemVo> list)
        {
            DebugUtils.Assert( cacheList == null, "cacheList should be null when invoking ShowList method!" );

            MatchSide mySide = MatchSide.Blue;
            long playerId = DataManager.GetInstance().GetPlayerId();
            for ( int i = 0; i < list.Count; i++ )
            {
                if ( list[i].playerID == playerId )
                {
                    mySide = list[i].sideIndex;
                    break;
                }
            }

            cacheList = new List<MatchLoadItem>();
            for (int i = 0; i < list.Count; i++)
            {
                MatchLoadItem item = Instantiate( match2V2LoadItem );

                if (list[i].sideIndex == mySide )
                {
                    item.transform.SetParent( tfFriendSideGroup, false );
                }
                else if (list[i].sideIndex != mySide )
                {
                    item.transform.SetParent( tfEnemySideGroup, false );
                }

                item.Init( list[i] );
                cacheList.Add( item );
            }
        }

        private int LoadingProgress()
        {
            switch (loadStep)
            {
                //case LoadStep.LoadingBaseScene:
                //{
                //    return (int)( async.progress * 30 );
                //}
                case LoadStep.LoadingBattleScene:
                {
                    return (int)( async.progress * 70 ) + 30;
                }
                case LoadStep.LoadingBattleMap:
                {
                    return (int)( resourceRequest.progress * 20 ) + 80;
                }
                default:
                {
                    DebugUtils.LogError( DebugUtils.Type.LoadingScene, string.Format( "LoadingProcess: the load step {0} isn't right!", loadStep ) );
                    return 0;
                }
            }
        }

        private void UpdataProgressByID(long id, int progress, NoticeType type = NoticeType.BattleLoading)
        {
            if (cacheList == null)
                return;
            
            for (int i = 0; i < cacheList.Count; i++)
            {
                if (id == cacheList[i].PlayerID)
                {
                    cacheList[i].Progress = progress;
                    cacheList[i].isLoadComplted = progress >= 100;
                    break;
                }
            }
        }

        private void UpdataProgressOther(long id, int progress)
        {
            for (int i = 0; i < cacheList.Count; i++)
            {
                if (id != cacheList[i].PlayerID)
                {
                    cacheList[i].Progress = progress;
                    cacheList[i].isLoadComplted = progress >= 100;
                }
            }
        }

        private void SendProgress(NoticeType noticeType, int progress)
        {
            NoticeC2S msg = new NoticeC2S();
            msg.type = noticeType;
            msg.loadingRate = progress;
            byte[] data = ProtobufUtils.Serialize( msg );
            NetworkManager.SendRequest( MsgCode.NoticeMessage, data );
        }

        private void OnNoticeRespond(byte[] data)
        {
            NoticeS2C msg = ProtobufUtils.Deserialize<NoticeS2C>( data );

            if (msg.type == NoticeType.BattleLoading )
            {
                int length = msg.loadingRatingInfos == null ? 0 : msg.loadingRatingInfos.Count;
                for (int i = 0; i < length; i++)
                {
                    UpdataProgressByID( msg.loadingRatingInfos[i].playerId, msg.loadingRatingInfos[i].playerLoadingRate );
                }
            }
            if (msg.type == NoticeType.BattleBegin)
            {
                DebugUtils.Assert( loadStep == LoadStep.Done, "Server notices the battle begins but the load step is " + loadStep );
                Invoke( "OnEnterSence", 2 );
            }
        }

        private void OnEnterSence()
        {
            GameObject.Destroy( gameObject );

            if ( battleType == BattleType.Tutorial )
            {
                PVE.TutorialModeManager.TutorialModeStage currentTurorialStage = DataManager.GetInstance().GetTutorialStage();

                if ( currentTurorialStage == PVE.TutorialModeManager.TutorialModeStage.NormallyControlOperation_Stage )
                {
                    MessageDispatcher.PostMessage( Constants.MessageType.OpenNewbieGuide, 0, 3 );
                }
                else if ( currentTurorialStage == PVE.TutorialModeManager.TutorialModeStage.BuildingControlOperation_Stage )
                {
                    MessageDispatcher.PostMessage( Constants.MessageType.OpenNewbieGuide, 0, 2 );
                }
            }
        }
    }
}