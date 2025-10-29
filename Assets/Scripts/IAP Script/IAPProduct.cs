using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MyPurchaseID
{
    // public const string RemoveAds = "com.minigamehub.removeads";
    public const string Pack1 = "com.crossword.pack1";
    public const string Pack2 = "com.crossword.pack2";
    public const string Pack3 = "com.crossword.pack3";
    public const string Pack4 = "com.crossword.pack4";
    public const string Pack5 = "com.crossword.pack5";
    public const string Pack6 = "com.crossword.pack6";
    public const string Pack7 = "com.crossword.pack7";
    public const string Pack8 = "com.crossword.pack8";
    public const string Pack9 = "com.crossword.pack9";
}

public class IAPProduct : MonoBehaviour
{
    [SerializeField] private string _purchaseID;
    [SerializeField] private Button _purchaseButton;
    [SerializeField] private TextMeshProUGUI _price;
    [SerializeField] private TextMeshProUGUI _discount;
    [SerializeField] private Sprite _icon;

    public string PurchaseID => _purchaseID;

    public delegate void PurchaseEvent(Product Model, Action OnComplete);

    public event PurchaseEvent OnPurchase;
    private Product _model;
    UIManager uiManager;

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        RegisterPurchase();
        RegisterEventButton();
    }

    protected virtual void RegisterPurchase()
    {
        StartCoroutine(IAPManager.Instance.CreateHandleProduct(this));
    }

    public void Setup(Product product, string code, string price)
    {
        _model = product;
        if (_price != null)
        {
            _price.text = price;
        }

        if (_discount != null)
        {
            if (code.Equals("VND"))
            {
                var round = Mathf.Round(float.Parse(price) + float.Parse(price) * .4f);
                _discount.text = code + " " + round;
            }
            else
            {
                var priceFormat = $"{float.Parse(price) + float.Parse(price) * .4f:0.00}";
                _discount.text = code + " " + priceFormat;
            }
        }
    }

    private void RegisterEventButton()
    {
        _purchaseButton.onClick.AddListener(() =>
        {
            //AudioManager.PlaySound("Click");
            Purchase();
        });
    }

    private void Purchase()
    {
        OnPurchase?.Invoke(_model, HandlePurchaseComplete);
    }

    private void HandlePurchaseComplete()
    {
        switch (_purchaseID)
        {
            // case MyPurchaseID.RemoveAds:
            //     RemoveAdsPack();
            //     break;
            case MyPurchaseID.Pack1:
                uiManager.BuyTicket(100);
                break;
            case MyPurchaseID.Pack2:
                uiManager.BuyTicket(200);
                break;
            case MyPurchaseID.Pack3:
                uiManager.BuyTicket(500);
                break;
            case MyPurchaseID.Pack4:
                uiManager.BuyTicket(700);
                break;
            case MyPurchaseID.Pack5:
                uiManager.BuyTicket(1100);
                break;
            case MyPurchaseID.Pack6:
                uiManager.BuyTicket(1700);
                break;
            case MyPurchaseID.Pack7:
                uiManager.BuyTicket(2500);
                break;
            case MyPurchaseID.Pack8:
                uiManager.BuyTicket(3000);
                break;
            case MyPurchaseID.Pack9:
                uiManager.BuyTicket(4000);
                break;
        }

        if (_icon != null)
        {
            _purchaseButton.gameObject.GetComponent<Image>().sprite = _icon;
            _purchaseButton.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
            _purchaseButton.interactable = false;
        }
    }
    
    private void AddCoin(int amount)
    {
        
    }
    
    private void RemoveAdsPack()
    {
        //ResourceManager.RemoveAds = true;
        // GameEventManager.PurchaseAds?.Invoke();
    }
}