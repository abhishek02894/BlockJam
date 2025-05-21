//using Firebase.Extensions;
//using Firebase.Messaging;
//using UnityEngine;

//namespace Tag
//{
//    public class FirebaseCloudMessaging : MonoBehaviour
//    {
//        #region PUBLIC_VARS
//        public string pushToken = "";
//        public bool isInit = false;

//        #endregion

//        #region private veriable

//        [SerializeField] private string topicName = "MergeTown";

//        #endregion

//        #region public methods

//        public void Init()
//        {
//            FirebaseMessaging.TokenReceived += OnTokenReceive;
//            isInit = true;
//            FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(task =>
//            {
//                pushToken = task.Result;
//            });
//        }

//        public void OnDestroy()
//        {
//            FirebaseMessaging.TokenReceived -= OnTokenReceive;
//        }

//        public void Subscribe()
//        {
//            FirebaseMessaging.SubscribeAsync(topicName);
//        }

//        public void Unsubscribe()
//        {
//            FirebaseMessaging.UnsubscribeAsync(topicName);
//        }
//        #endregion

//        #region private methods

//        private void OnTokenReceive(object sender, TokenReceivedEventArgs token)
//        {
//            Subscribe();
//            Debug.LogError("PlayFab: Received Registration Token: " + token.Token);
//            pushToken = token.Token;
//        }

//        #endregion
//    }
//}