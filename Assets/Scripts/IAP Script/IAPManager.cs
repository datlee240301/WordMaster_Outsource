using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : Singleton<IAPManager>
{
    public event Action<List<Product>> OnProductsFetcheded;
    public event Action<string> OnPurchaseSuccessful;
    
    [SerializeField] private bool UseFakeStore = false;
    private StoreController _storeController;
    private bool _validatePurchases;
    private bool _areProductsFetched;
    private Action _pendingCallback;

    private async void Awake()
    {
        InitializationOptions options = new InitializationOptions()
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            .SetEnvironmentName("test");
#else
            .SetEnvironmentName("production");
#endif
        await UnityServices.InitializeAsync(options);
        InitializeIAP();
    }
    
    private void InitializeIAP()
    {
        _storeController = UnityIAPServices.StoreController();
        ConfigureCallbacks();
        Connect();
    }
    
    private async void Connect()
    {
        await _storeController.Connect();
        Debug.Log("IAP system successfully initialized and ready to make purchases.");
        var initialProductsToFetch = BuildProductDefinitions();
        _storeController.FetchProducts(initialProductsToFetch);
    }
    
    private List<ProductDefinition> BuildProductDefinitions()
    {
        return new List<ProductDefinition>
        {
            new ProductDefinition(MyPurchaseID.Pack1, ProductType.Consumable),
            new ProductDefinition(MyPurchaseID.Pack2, ProductType.Consumable),
            new ProductDefinition(MyPurchaseID.Pack3, ProductType.Consumable),
            new ProductDefinition(MyPurchaseID.Pack4, ProductType.Consumable),
            new ProductDefinition(MyPurchaseID.Pack5, ProductType.Consumable),
            new ProductDefinition(MyPurchaseID.Pack6, ProductType.Consumable),
            new ProductDefinition(MyPurchaseID.Pack7, ProductType.Consumable),
            new ProductDefinition(MyPurchaseID.Pack8, ProductType.Consumable),
            new ProductDefinition(MyPurchaseID.Pack9, ProductType.Consumable),
        };
    }
    
    private void ConfigureCallbacks()
    {
        _storeController.OnStoreDisconnected += OnStoreDisconnected;
        _storeController.OnProductsFetched += OnProductsFetched;
        _storeController.OnProductsFetchFailed += OnProductsFetchFailed;
        _storeController.OnPurchasesFetched += OnPurchasesFetched;
        _storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
        _storeController.OnPurchasePending += OnPurchasePending;
        _storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
        _storeController.OnPurchaseFailed += OnPurchaseFailed;
    }
    
    public bool TryGetProduct(string productId, out Product product)
    {
        if (!_areProductsFetched)
        {
            product = null;
            return false;
        }
        product = _storeController.GetProductById(productId);
        if (product == null)
        {
            Debug.LogError($"Could not get the product with id: {productId}.");
            return false;
        }
        return true;
    }
    
    public void Purchase(string productId)
    {
        if (!_areProductsFetched)
        {
            Debug.LogWarning($"Purchase of product failed. System not connected or failed to fetch product with id '{productId}'.");
            return;
        }
        if (!TryGetProduct(productId, out var product))
        {
            Debug.LogWarning($"A product with id '{productId}' was not fetched or it's fetching failed.");
            return;
        }
        if (product is { availableToPurchase: false })
        {
            Debug.LogWarning("Purchase of product failed. Not purchasing product, as the product is either not found or is not available for purchase.");
            return;
        }
        // Buy the product. Expect a response either through OnPurchasePending or the failure events.
        Debug.Log($"Purchasing product asychronously: '{product.definition.id}'");
        _storeController.PurchaseProduct(product);
    }
    
    public bool HasProductBeenFetched(string productId, out Product product)
    {
        if (!_areProductsFetched)
        {
            product = null;
            return false;
        }
        product = _storeController.GetProductById(productId);
        return product != null;
    }
    
    public void RestoreTransactions()
    {
        _storeController.RestoreTransactions((result, error) =>
        {
            if (result)
            {
                // This does not mean anything was restored, merely that the restoration process succeeded.
                // During this process the ProcessPurchase method of IStoreListener will be invoked for any items the user already owns.
                Debug.Log("Restoring transactions...");
            }
            else
            {
                Debug.LogWarning($"Restoration failed, error: {error}");
                
            }
        });
    }
    
    private void OnStoreDisconnected(StoreConnectionFailureDescription desc)
    {
        Debug.LogError($"Failed to initialize the IAP system, failure: {desc.message}.");
    }

    private void OnProductsFetched(List<Product> products)
    {
        _areProductsFetched = true;
        // By default, calling FetchPurchases invokes OnPurchasePending for any pending purchases which have not yet been handled in the session.
        // You can disable this behaviour with StoreController.ProcessPendingOrdersOnPurchasesFetched(false).
        _storeController.FetchPurchases();
        OnProductsFetcheded?.Invoke(products); // The list has only the products that were fetched correctly.
    }
    
    private void OnProductsFetchFailed(ProductFetchFailed fail)
    {
        string failureMsg = "Failed to fetch the following products:\n";
        foreach (var failedProduct in fail.FailedFetchProducts)
        {
            failureMsg += failedProduct.id + "\n";
        }
        Debug.LogError($"{failureMsg}, failure: {fail.FailureReason}.");
    }
    
    private void OnPurchasesFetched(Orders orders)
    {
        // Fetched purchase(s).
    }
    
    private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription desc)
    {
        Debug.LogError($"Failed to fetch the purchases, failure: {desc.message}.");
    }
    
    private void OnPurchasePending(PendingOrder pendingOrder)
    {
        // Presume valid for platforms with no receipt validator.
        bool validPurchase = true;
        // Unity IAP's validation logic is only included on these platforms.
        if (_validatePurchases)
        {
#if USING_IAP
            validPurchase = _crossPlatformObfuscationValidator.Validate(pendingOrder);
#endif
        }
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (validPurchase)
        {
            foreach (var cartItem in pendingOrder.CartOrdered.Items())
            {
                var product = cartItem.Product;
                var id = product.definition.id;
                // Unlock the appropriate content here.
                // Content will be automatically restored on supported platforms.
                // We don't bother with storeSpecificIds, as we use the common ids for everything.
                Debug.LogError($"Purchased Product: '{id}'");
            }
        }
        // We call CompletePurchase, informing Unity IAP that the processing on our side is done and the transaction can be closed.
        _storeController.ConfirmPurchase(pendingOrder);
    }
    
    private void OnPurchaseConfirmed(Order order)
    {
        switch (order)
        {
            case ConfirmedOrder confirmedOrder:
                foreach (var cartItem in confirmedOrder.CartOrdered.Items())
                {
                    var product = cartItem.Product;
                    Debug.LogError($"Successfully confirmed order: {product.definition.id}");
                    SoundManager.instance.PlayGetCoinSound();
                    _pendingCallback?.Invoke();
                    _pendingCallback = null;
                }
                break;
            case FailedOrder failedOrder:
                var reason = failedOrder.FailureReason;
                Debug.LogError($"Order failed: {reason}");
                foreach (var cartItem in failedOrder.CartOrdered.Items())
                {
                    var product = cartItem.Product;
                    Debug.LogError($"Failed to confirm order for product: {product.definition.id}");
                }
                break;
        }
    }
    
    private void OnPurchaseFailed(FailedOrder failedOrder)
    {
        var reason = failedOrder.FailureReason.ToString();
        Debug.LogError($"Purchase failed: {reason}");
    }

    public IEnumerator CreateHandleProduct(IAPProduct pack)
    {
        List<Product> sortedProducts = _storeController.GetProducts()
            .TakeWhile(item => !item.definition.id.Contains("sale"))
            .OrderBy(item => item.metadata.localizedPrice)
            .ToList();
        foreach (Product product in sortedProducts)
        {
            if (pack.PurchaseID == product.definition.id)
            {
                var code = "";
                var price = "";
                code = product.metadata.isoCurrencyCode;
                price = product.metadata.localizedPriceString;
                pack.OnPurchase += HandlePurchase;
                pack.Setup(product, code, price);
            }
        }
        yield return null;
    }
    
    private void HandlePurchase(Product product, Action onComplete)
    {
        _storeController.PurchaseProduct(product);
        _pendingCallback = onComplete;
    }
}
