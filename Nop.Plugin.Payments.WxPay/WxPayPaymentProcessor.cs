using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.WxPay.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;
using System.Web;
using System.Net.Http;
using Nop.Plugin.Payments.WxPay;
using Nop.Services.Vendors;
using Nop.Plugin.Payments.WxPay.Models;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Controllers;
using Nop.Plugin.Payments.WxPay.app_code;
using Nop.Services.Orders;

namespace Nop.Plugin.Payments.WxPay
{
    /// <summary>
    /// AliPay payment processor
    /// </summary>
    public class WxPayPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Constants

        private const string ShowUrl = "http://www.alipay.com/";
        private const string Service = "create_direct_pay_by_user";
        private const string SignType = "MD5";
        private const string InputCharset = "utf-8";

        #endregion

        #region Fields

        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private readonly WxPayPaymentSettings _WxPaymentSettings;
        private readonly IVendorService _vendorService;
        private readonly IOrderService _orderService;
        private readonly IRefundOrderItemService _refundOrderItemService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public WxPayPaymentProcessor(
            ISettingService settingService,
            IWebHelper webHelper,
            IStoreContext storeContext,
            WxPayPaymentSettings wxPayPaymentSettings,
            IVendorService vendorService,
            IOrderService orderService,
            IRefundOrderItemService refundOrderItemService,
            IWorkContext workContext)
        {
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._storeContext = storeContext;
            this._WxPaymentSettings = wxPayPaymentSettings;
            this._vendorService = vendorService;
            this._orderService = orderService;
            this._refundOrderItemService = refundOrderItemService;
            this._workContext = workContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets MD5 hash
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="inputCharset">Input charset</param>
        /// <returns>Result</returns>
        internal string GetMD5(string prestr, string inputCharset)
        {
            
            StringBuilder sb = new StringBuilder(32);

            //prestr = prestr + _aliPayPaymentSettings.Key;

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] t = md5.ComputeHash(Encoding.GetEncoding(inputCharset).GetBytes(prestr));
            for (int i = 0; i < t.Length; i++)
            {
                sb.Append(t[i].ToString("x").PadLeft(2, '0'));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Create URL
        /// </summary>
        /// <param name="para">Para</param>
        /// <param name="inputCharset">Input charset</param>
        /// <param name="key">Key</param>
        /// <returns>Result</returns>
        private string CreatUrl(string[] para, string inputCharset, string key)
        {
            Array.Sort(para, StringComparer.InvariantCulture);

            int i;
            var prestr = new StringBuilder();

            for (i = 0; i < para.Length; i++)
            {
                prestr.Append(para[i]);

                if (i < para.Length - 1)
                {
                    prestr.Append("&");
                }
            }

            prestr.Append(key);

            var sign = GetMD5(prestr.ToString(), inputCharset);

            return sign;
        }

        /// <summary>
        /// Gets HTTP
        /// </summary>
        /// <param name="strUrl">Url</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>Result</returns>
        internal string GetHttp(string strUrl, int timeout)
        {
            var strResult = string.Empty;

            try
            {
                var myReq = (HttpWebRequest)WebRequest.Create(strUrl);

                myReq.Timeout = timeout;

                var httpWResp = (HttpWebResponse)myReq.GetResponse();
                var myStream = httpWResp.GetResponseStream();
                if (myStream != null)
                {
                    using (var sr = new StreamReader(myStream, Encoding.Default))
                    {
                        var strBuilder = new StringBuilder();

                        while (-1 != sr.Peek())
                        {
                            strBuilder.Append(sr.ReadLine());
                        }

                        strResult = strBuilder.ToString();
                    }
                }
            }
            catch (Exception exc)
            {
                strResult = string.Format("Error: {0}", exc.Message);
            }

            return strResult;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Pending };

            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //��ȡ������ID
            ICollection<OrderItem> list = postProcessPaymentRequest.Order.OrderItems;
            ConfigurationModel wenxinConfig = new ConfigurationModel();
            var modelList = postProcessPaymentRequest.Order.OrderItems as List<OrderItem>;
            // var totalFee = postProcessPaymentRequest.Order.OrderTotal.ToString("0.00", CultureInfo.InvariantCulture);
            var vendorId = modelList[0].Product.VendorId;
            var wenxin = _vendorService.GetVendorById(vendorId);
            wenxinConfig.APPID = wenxin.APPID.Trim();
            wenxinConfig.APPSECRET = wenxin.APPSECRET.Trim();
            wenxinConfig.KEY = wenxin.KEY.Trim();
            wenxinConfig.MCHID = wenxin.MCHID.Trim();
            wenxinConfig.IP = postProcessPaymentRequest.Order.CustomerIp;
            OrderDetails orderdetails = new OrderDetails
            {
                Attach = wenxin.Name,
                OrderId = postProcessPaymentRequest.Order.Id,
                Body = modelList[0].Product.Name,
                Detail = modelList[0].Product.ShortDescription,
                ProductId = modelList[0].Product.Id.ToString(),
                Total_fee = (Convert.ToDouble(postProcessPaymentRequest.Order.OrderTotal) * 100).ToString()
            };
            wenxinConfig.orderDetails = orderdetails;
            NativePayHtml htmlPay = new NativePayHtml(wenxinConfig);
            string htmPay = htmlPay.GetHtmlPay();
            string jsPay = htmlPay.GetPayJs();
            var post = new RemotePost();
            post.Post(htmPay,jsPay);
        }
        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// ��ö���������
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _WxPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// ��׽����
        /// </summary>
        /// <param name="capturePaymentRequest">��׽����֧��</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();

            result.AddError("��֧�ֲ��񷽷�");
            return result;
        }

        /// <summary>
        /// �˿�֧��
        /// </summary>
        /// <param name="refundPaymentRequest">����</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            //��ȡ������ID
            ICollection<OrderItem> list = refundPaymentRequest.Order.OrderItems;
            ConfigurationModel wenxinConfig = new ConfigurationModel();
            var modelList = refundPaymentRequest.Order.OrderItems as List<OrderItem>;
            // var totalFee = postProcessPaymentRequest.Order.OrderTotal.ToString("0.00", CultureInfo.InvariantCulture);
            var vendorId = modelList[0].Product.VendorId;
            var wenxin = _vendorService.GetVendorById(vendorId);
            wenxinConfig.APPID = wenxin.APPID.Trim();
            wenxinConfig.APPSECRET = wenxin.APPSECRET.Trim();
            wenxinConfig.KEY = wenxin.KEY.Trim();
            wenxinConfig.MCHID = wenxin.MCHID.Trim();
            wenxinConfig.IP = refundPaymentRequest.Order.CustomerIp;
            //wenxinConfig.SSLCERT_PATH = _webHelper.GetStoreLocation(false) + wenxin.SSLCERT_PATH.Trim().Substring(1, wenxin.SSLCERT_PATH.Trim().Length - 1);
            wenxinConfig.SSLCERT_PATH = wenxin.SSLCERT_PATH.Trim();
            wenxinConfig.SSLCERT_PASSWORD = wenxin.MCHID.Trim();
            OrderDetails orderdetails = new OrderDetails
            {
                Attach = wenxin.Name,
                OrderId = refundPaymentRequest.Order.Id,
                Body = modelList[0].Product.Name,
                Detail = modelList[0].Product.ShortDescription,
                ProductId = modelList[0].Product.Id.ToString(),
                Total_fee = (Convert.ToDouble(refundPaymentRequest.Order.OrderTotal) * 100).ToString()
            };
            wenxinConfig.orderDetails = orderdetails;
            //�˿�
            string amountToRefund = Convert.ToInt32(refundPaymentRequest.AmountToRefund * 100).ToString();
            Refund refund = new Refund(wenxinConfig);
            //���ö����˿�ӿ�,����ڲ������쳣����ҳ������ʾ�쳣ԭ��
            if (refundPaymentRequest.Order.WxTransactionId == null)
            {
                result.AddError("û�в�ѯ��΢��֧������");
                return result;
            }
            try
            {
                string returnMess = refund.Run(refundPaymentRequest.Order.WxTransactionId, refundPaymentRequest.Order.Id.ToString(), orderdetails.Total_fee, amountToRefund);
                string[] cpTypeArray = returnMess.Replace("<br>", ",").Split(',');
                WxPayData res = new WxPayData();
                foreach (var item in cpTypeArray)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                        res.SetValue(item.Split('=')[0], item.Split('=')[1]);
                }
                if (res.GetValue("return_code").ToString() != "SUCCESS" || res.GetValue("result_code").ToString() != "SUCCESS")
                {
                    //�˿���ʾ
                    result.AddError(res.GetValue("err_code_des").ToString());
                    return result;
                }
                else
                {
                    //����˿��¼��
                    RefundOrderItem item = new RefundOrderItem();
                    item.OrderId = refundPaymentRequest.Order.Id;
                    item.CustomNumber = refundPaymentRequest.Order.Customer.Id;
                    item.VendorId = vendorId;
                    item.WxTransactionId = res.GetValue("transaction_id").ToString();
                    item.WxRefunId = res.GetValue("refund_id").ToString();
                    item.WxOutRefunNo = res.GetValue("out_refund_no").ToString();
                    item.OrderTotal = refundPaymentRequest.Order.OrderTotal;
                    item.RefundedAmount = refundPaymentRequest.AmountToRefund;
                    item.CreatedOnUtc = System.DateTime.Now;
                    item.Deleted = false;
                    _refundOrderItemService.InsertRefundOrderItem(item);
                }
            }
            catch (WxPayException ex)
            {
                result.AddError(ex.ToString());
            }
            catch (Exception ex)
            {
                result.AddError(ex.ToString());
            }
            return result;
        }

        //�޸Ķ�����Ϣ
        public void UpdateOrderPayStaus(int orderId)
        {
            var orderModel = _orderService.GetOrderById(orderId);
            orderModel.PaymentStatus = PaymentStatus.Refunded;
            _orderService.UpdateOrder(orderModel);

        }


        /// <summary>
        /// �յ�֧��
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();

            result.AddError("Void method not supported");

            return result;
        }

        /// <summary>
        ///���̵ľ�����֧��
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            result.AddError("Recurring payment not supported");

            return result;
        }

        /// <summary>
        /// ȡ�����ڸ���
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();

            result.AddError("��֧�־����Ը���");

            return result;
        }

        /// <summary>
        /// ��ȡһ��ֵ����ֵָʾ�ͻ��Ƿ�����ڶ��������õ�δ��ɺ���ɸ���ض���֧��������
        /// </summary>
        /// <param name="order">����</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("����");

            //AliPay���ض���֧������
            //��Ҳ��֤�����Ƿ�Ҳ֧�����ض���󣩣������ͻ��Ͳ���֧������
            //����״̬Ӧ����
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;

            //������ȷ������������1����ͨ��
            return !((DateTime.Now - order.CreatedOnUtc).TotalMinutes < 1);
        }

        /// <summary>
        /// ��ȡ�ṩ�����õ�·��
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "WxPay";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.WxPay.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// ��ȡ������Ϣ��·��
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "WxPay";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.WxPay.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(WxPayController);
        }

        public override void Install()
        {
            //settings
            var settings = new WxPayPaymentSettings()
            {
                APPID = "",
                KEY = "",
                APPSECRET = "",
                AdditionalFee = 0,
            };

            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.RedirectionTip", "You will be redirected to AliPay site to complete the order.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.SellerEmail", "Seller email");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.SellerEmail.Hint", "Enter seller email.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.Key", "Key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.Key.Hint", "Enter key.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.Partner", "Partner");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.Partner.Hint", "Enter partner.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.AdditionalFee.Hint", "Enter additional fee to charge your customers.");

            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.SellerEmail.RedirectionTip");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.SellerEmail");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.SellerEmail.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Key");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Key.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Partner");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Partner.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.AdditionalFee.Hint");

            base.Uninstall();
        }

        decimal IPaymentMethod.GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _WxPaymentSettings.AdditionalFee;
        }

        #endregion

        #region Properies

        /// <summary>
        /// ��ȡһ��ֵ����ֵָʾ�Ƿ�֧�ֲ���
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// ��ȡһ��ֵ����ֵָʾ�Ƿ�֧�ֲ����˿�
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// ��ȡһ��ֵ����ֵָʾ�Ƿ�֧���˿�
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// ��ȡһ��ֵ����ֵָʾ�Ƿ�֧�ֿհ�
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// ��ȡ���ڸ������͵ĸ����
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        ///��ȡ���������
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        /// <summary>
        /// ��ֵָʾ�Ƿ�Ӧ��ʾ������֧����Ϣҳ��
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        #endregion
    }
}
