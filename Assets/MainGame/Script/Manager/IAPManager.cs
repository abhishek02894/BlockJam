using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace Tag.Block {
    public class IAPManager : SerializedManager<IAPManager>, IStoreListener
    {
        #region private veriables

        [SerializeField] private IapProductIdsConfig iapProductIds = new IapProductIdsConfig();

        private static IStoreController m_StoreController; // The Unity Purchasing system.

        private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.
        private Action<string> successAction, failedAction;

        public int IapPurchaseCount
        {
            get { return PlayerPrefs.GetInt("IapPurchaseCountt", 0); }
            private set { PlayerPrefs.SetInt("IapPurchaseCountt", value); }
        }

        #endregion

        #region unity callback

        private void Start()
        {
            InternetManager.Instance.CheckNetConnection(OnNetCheckFinish);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            m_StoreController = null;
            m_StoreExtensionProvider = null;
        }
        #endregion

        #region private methods

        private void OnNetCheckFinish()
        {
            if (m_StoreController == null)
            {
                InitializePurchasing();
            }
        }

        private bool IsInitialized()
        {
            // Only say we are initialized if both the Purchasing references are set.
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }

        private void BuyProductID(string productId)
        {
            // If Purchasing has been initialized ...
            if (IsInitialized())
            {
                // ... look up the Product reference with the general product identifier and the Purchasing 
                // system's products collection.
                Product product = m_StoreController.products.WithID(productId);

                // If the look up found a product for this device's store and that product is ready to be sold ... 
                if (product != null && product.availableToPurchase)
                {
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                    // asynchronously.
                    m_StoreController.InitiatePurchase(product);
                }
                // Otherwise ...
                else
                {
                    // ... report the product look-up failure situation  
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            // Otherwise ...
            else
            {
                //GameplayUIManager.Instance.GetView<InfoView>().Show(ToastMessageConstant.PurchaseFailedTitle, ToastMessageConstant.PurchaseFailedMessage);
                OnPurchaseFailed(productId);
                failedAction = null;
                successAction = null;
                // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
                // retrying initiailization.
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }
        private void OnPurchaseSuccess(PurchaseEventArgs productId)
        {
            if (productId.purchasedProduct.hasReceipt)
            {
                if (successAction != null)
                    successAction?.Invoke(productId.purchasedProduct.definition.id);
            }
            else
            {
                Debug.LogError("Purchase does not have a receipt");
            }
        }
        
        
        private PurchaseReceipt GetPurchaseReceipt(PurchaseEventArgs args)
        {
            // args is received from the processPurchase methos
            string receipt = args.purchasedProduct.receipt;
            string productId = args.purchasedProduct.definition.id;
            string transactionId = args.purchasedProduct.transactionID;
            string purchaseToken = args.purchasedProduct.transactionID;
            //string developerPayload = string.Empty;
            Dictionary<string, object> receiptDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receipt);
            if (receiptDict.ContainsKey("Payload"))
            {
                string payloadStr = (string)receiptDict["Payload"];
                Dictionary<string, object> payload = JsonConvert.DeserializeObject<Dictionary<string, object>>(payloadStr);

                if (payload.ContainsKey("json"))
                {
                    string payloadJsonStr = (string)payload["json"];
                    Dictionary<string, object> payloadJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(payloadJsonStr);
                    if (payloadJson.ContainsKey("orderId"))
                    {
                        transactionId = (string)payloadJson["orderId"];
                    }
                }
            }
            return new PurchaseReceipt(productId, transactionId, purchaseToken);//, developerPayload);
        }
        private void OnPurchaseFailed(string productId)
        {
            if (failedAction != null)
                failedAction?.Invoke(productId);
            //GlobalUIManager.Instance.HideView<InGameLoadingView>();
        }

        double GetIAPPrice(PurchaseEventArgs args)
        {
            return (double)args.purchasedProduct.metadata.localizedPrice;
        }

        string GetIAPISOCode(PurchaseEventArgs args)
        {
            return args.purchasedProduct.metadata.isoCurrencyCode;
        }
        #endregion

        #region public methods

        public void InitializePurchasing()
        {
            // If we have already connected to Purchasing ...
            if (IsInitialized())
            {
                // ... we are done here.
                OnLoadingDone();
                return;
            }

            // Create a builder, first passing in a suite of Unity provided stores.
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // Add a product to sell / restore by way of its identifier, associating the general identifier
            // with its store-specific identifiers.
            for (int i = 0; i < iapProductIds.consumableProductIds.Count; i++)
            {
                builder.AddProduct(iapProductIds.consumableProductIds[i], ProductType.Consumable);
            }

            // Continue adding the non-consumable product.
            for (int i = 0; i < iapProductIds.nonConsumableProductIds.Count; i++)
            {
                builder.AddProduct(iapProductIds.nonConsumableProductIds[i], ProductType.NonConsumable);
            }

            // Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
            // and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
            UnityPurchasing.Initialize(this, builder);
        }


        // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
        // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
        public void RestorePurchases()
        {
            // If Purchasing has not yet been set up ...
            if (!IsInitialized())
            {
                // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
                Debug.Log("RestorePurchases FAIL. Not initialized.");
                return;
            }

            // If we are running on an Apple device ... 
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                // ... begin restoring purchases
                Debug.Log("RestorePurchases started ...");

                // Fetch the Apple store-specific subsystem.
                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                apple.RestoreTransactions((result) =>
                {
                    // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                    // no purchases are available to be restored.
                    Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
                });
            }
            // Otherwise ...
            else
            {
                // We are not running on an Apple device. No work is necessary to restore purchases.
                Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }


        //  
        // --- IStoreListener
        //

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            // Purchasing has succeeded initializing. Collect our Purchasing references.
            Debug.Log("OnInitialized: PASS");

            // Overall Purchasing system, configured with products for this application.
            m_StoreController = controller;
            // Store specific subsystem, for accessing device-specific store features.
            m_StoreExtensionProvider = extensions;

            OnLoadingDone();
        }


        public KeyValuePair<string, string> GetLocalPrice(string productId)
        {
            if (m_StoreController != null)
            {
                Product product = m_StoreController.products.WithID(productId);
                if (product == null)
                    return new KeyValuePair<string, string>("None", "None");
                return new KeyValuePair<string, string>(product.metadata.localizedPrice + "", product.metadata.isoCurrencyCode);
            }

            return new KeyValuePair<string, string>("None", "None");
        }


        public void OnInitializeFailed(InitializationFailureReason error)
        {
            // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
            OnLoadingDone();
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
            OnLoadingDone();
        }
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            bool isIDValid = false;
            // A consumable product has been purchased by this user.

            for (int i = 0; i < iapProductIds.consumableProductIds.Count; i++)
            {
                if (iapProductIds.consumableProductIds[i].Equals(args.purchasedProduct.definition.id))
                {
                    isIDValid = true;
                    OnPurchaseSuccess(args);
                    break;
                }
            }

            // Or ... a non-consumable product has been purchased by this user.
            for (int i = 0; i < iapProductIds.nonConsumableProductIds.Count; i++)
            {
                if (iapProductIds.nonConsumableProductIds[i].Equals(args.purchasedProduct.definition.id))
                {
                    isIDValid = true;
                    OnPurchaseSuccess(args);
                    break;
                }
            }

            if (!isIDValid)
            {
                //GameplayUIManager.Instance.GetView<InfoView>().Show(ToastMessageConstant.PurchaseFailedTitle, ToastMessageConstant.PurchaseFailedMessage);
                OnPurchaseFailed(args.purchasedProduct.definition.id);
            }

            failedAction = null;
            successAction = null;

            // Return a flag indicating whether this product has completely been received, or if the application needs 
            // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
            // saving purchased products to the cloud, and when that save is delayed. 
            return PurchaseProcessingResult.Complete;
        }


        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
            // this reason with the user to guide their troubleshooting actions.
            //GameplayUIManager.Instance.GetView<InfoView>().Show(ToastMessageConstant.PurchaseFailedTitle, ToastMessageConstant.PurchaseFailedMessage);
            OnPurchaseFailed(product.definition.id);
            failedAction = null;
            successAction = null;
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        }

        public void PurchaseProduct(string productId, Action<string> onSuccess, Action<string> onFailed)
        {
            Debug.LogError("DeviceId: " + DeviceManager.Instance.GetDeviceID());
            Debug.LogError("productId: " + productId);
            //GlobalUIManager.Instance.GetView<InGameLoadingView>().Show("Connecting to Store");
            if (IsInitialized())
            {
                successAction = onSuccess;
                failedAction = onFailed;
                BuyProductID(productId);
            }
            else
            {
                InitializePurchasing();
                OnPurchaseFailed(productId);
            }
        }
        #endregion

#if UNITY_EDITOR
        [Button]
        public string GetJosn()
        {
            return JsonConvert.SerializeObject(iapProductIds);
        }

        [Button]
        public void SetJosn(string json)
        {
            iapProductIds = JsonConvert.DeserializeObject<IapProductIdsConfig>(json);
        }
#endif
    }

    public class IapProductIdsConfig
    {
        public List<string> consumableProductIds = new List<string>();
        public List<string> nonConsumableProductIds = new List<string>();
    }
    public class GABusinessEventDataMapping
    {
        public string name;
        public string itemType;
        public string itemId;
        public string cardType;
    }
    public struct PurchaseReceipt
    {
        public readonly string productId;
        public readonly string transactionId;
        public readonly string purchaseToken;
        //public readonly string developerPayload;
        public PurchaseReceipt(string productId, string transactionId, string purchaseToken)//, string developerPayload)
        {
            this.productId = productId;
            this.transactionId = transactionId;
            this.purchaseToken = purchaseToken;
            //this.developerPayload = developerPayload;
        }
    }
}