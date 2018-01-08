using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using DaBoLang.Nop.Plugin.Payments.AliPay.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Com.Alipay;
using DaBoLang.Nop.Plugin.Payments.AliPay.Data;
using DaBoLang.Nop.Plugin.Payments.AliPay.Domain;
using DaBoLang.Nop.Plugin.Payments.AliPay.Services;

namespace DaBoLang.Nop.Plugin.Payments.AliPay
{
    /// <summary>
    /// �����ռ䣺DaBoLang.Nop.Plugin.Payments.AliPay
    /// ��    �ƣ�AliPayPaymentProcessor
    /// ��    �ܣ�֧�����
    /// ��    ϸ��֧������ʱ���˲��
    /// ��    ����1.0.0.0
    /// �ļ����ƣ�AliPayPaymentProcessor.cs
    /// ����ʱ�䣺2017-08-02 01:37
    /// �޸�ʱ�䣺2017-08-04 01:40
    /// ��    �ߣ�����
    /// ��ϵ��ʽ��http://www.cnblogs.com/yaoshangjin
    /// ˵    ����
    /// </summary>
    public class AliPayPaymentProcessor : BasePlugin, IPaymentMethod
    {

        #region ����

        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private readonly AliPayPaymentSettings _aliPayPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly AliPayObjectContext _objectContext;
        private readonly IPaymentInfoService _paymentInfoService;
        private readonly IRefundInfoService _refundInfoService;
        #endregion

        #region ����

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settingService"></param>
        /// <param name="webHelper"></param>
        /// <param name="storeContext"></param>
        /// <param name="aliPayPaymentSettings"></param>
        /// <param name="localizationService"></param>
        /// <param name="workContext"></param>
        /// <param name="objectContext"></param>
        /// <param name="paymentInfoService"></param>
        /// <param name="refundInfoService"></param>
        public AliPayPaymentProcessor(
            ISettingService settingService,
            IWebHelper webHelper,
            IStoreContext storeContext,
            AliPayPaymentSettings aliPayPaymentSettings,
            ILocalizationService localizationService,
            IWorkContext workContext,
            AliPayObjectContext objectContext,
            IPaymentInfoService paymentInfoService,
            IRefundInfoService refundInfoService)
        {
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._storeContext = storeContext;
            this._aliPayPaymentSettings = aliPayPaymentSettings;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._objectContext = objectContext;
            this._paymentInfoService = paymentInfoService;
            this._refundInfoService = refundInfoService;
        }

        #endregion
        #region ��������
        /// <summary> 
        /// ����GUID��ȡ19λ��Ψһ�������� 
        /// </summary> 
        /// <returns></returns> 
        public static long GuidToLongID()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }
        #endregion
        #region ����

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

        #region ֧��
        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var partner = _aliPayPaymentSettings.Partner;

            if (string.IsNullOrEmpty(partner))
                throw new Exception("���������ID ����Ϊ��");

            var key = _aliPayPaymentSettings.Key;

            if (string.IsNullOrEmpty(key))
                throw new Exception("MD5��Կ����Ϊ��");

            var sellerEmail = _aliPayPaymentSettings.SellerEmail;

            if (string.IsNullOrEmpty(sellerEmail))
                throw new Exception("����Email ����Ϊ��");

            var customer = _workContext.CurrentCustomer;//��ǰ�û�
            string username = customer.Username;


            //�̻������ţ��̻���վ����ϵͳ��Ψһ�����ţ�����
            string out_trade_no = postProcessPaymentRequest.Order.Id.ToString().Trim();//�������

            //�������ƣ�����
            string subject = _storeContext.CurrentStore.Name + ":����" + out_trade_no;

            //���������
            string total_fee = postProcessPaymentRequest.Order.OrderTotal.ToString("0.00", CultureInfo.InvariantCulture);

            //��Ʒ�������ɿ�
            string body = _storeContext.CurrentStore.Name + ":�û�_" + username;

            //֧��������Ϣ
            var aliPayDirectConfig = new AlipayDirectConfig()
            {
                key = _aliPayPaymentSettings.Key,
                partner = _aliPayPaymentSettings.Partner,
                seller_email = _aliPayPaymentSettings.SellerEmail,
                notify_url = _webHelper.GetStoreLocation(false) + "Plugins/AliPay/Notify",
                return_url = _webHelper.GetStoreLocation(false) + "Plugins/AliPay/Return",
                sign_type = "MD5",
                input_charset= "utf-8",
            };  
            //������������������
            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            sParaTemp.Add("service", aliPayDirectConfig.service);
            sParaTemp.Add("partner", aliPayDirectConfig.partner);
            sParaTemp.Add("seller_email", aliPayDirectConfig.seller_email);
            sParaTemp.Add("payment_type", aliPayDirectConfig.payment_type);
            sParaTemp.Add("notify_url", aliPayDirectConfig.notify_url);
            sParaTemp.Add("return_url", aliPayDirectConfig.return_url);
            sParaTemp.Add("_input_charset", aliPayDirectConfig.input_charset);
            sParaTemp.Add("out_trade_no", out_trade_no);
            sParaTemp.Add("subject", subject);
            sParaTemp.Add("body", body);
            sParaTemp.Add("total_fee", total_fee);
            //����֧��������
            var post = AlipaySubmit.BuildRequest(sParaTemp, aliPayDirectConfig, "POST");
            post.Post();
           
        }
        #endregion
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
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _aliPayPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();

            result.AddError("Capture method not supported");

            return result;
        }

        #region �˿�
        /// <summary>
        /// �˿�
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            Order order = refundPaymentRequest.Order;
            if (order == null)
            {
                result.AddError("����Ϊ��");
                return result;
            }
            PaymentInfo paymentInfo = _paymentInfoService.GetByOrderId(order.Id);
            if (!(paymentInfo != null && !string.IsNullOrEmpty(paymentInfo.Out_Trade_No)))
            {
                result.AddError("���׺�Ϊ��");
                return result;
            }
            if (_aliPayPaymentSettings.Partner != paymentInfo.Seller_id)
            {
                result.AddError("�˿���������ID����");
                return result;
            }
            if (refundPaymentRequest.AmountToRefund <= 0)
            {
                result.AddError("�˿������0");
                return result;
            }
            if (refundPaymentRequest.AmountToRefund + refundPaymentRequest.Order.RefundedAmount > paymentInfo.Total)
            {
                result.AddError("�˿������");
                return result;
            }

            //�����˺�,�˿��˺�
            string seller_emailToRefund = paymentInfo.Seller_email;// �����˿��˺�����
            string seller_user_id = paymentInfo.Seller_id;//�����˿��˺�ID

            //���κţ������ʽ����������[8λ]+���к�[3��24λ]���磺201603081000001

            string batch_no = DateTime.Now.ToString("yyyyMMdd") + GuidToLongID();//�˿���

            //�˿�������������detail_data��ֵ�У���#���ַ����ֵ�������1�����֧��1000�ʣ�����#���ַ����ֵ�����999����

            string batch_num = "1";

            //�˿���ϸ���ݣ������ʽ��֧�������׺�^�˿���^��ע�����������#����
            string out_trade_no = paymentInfo.Out_Trade_No;//֧�������׺Ž��׺�
            string amountToRefund = refundPaymentRequest.AmountToRefund.ToString().TrimEnd('0');//�˿���
            string refundResult = "Э���˿�";//��ע
            string detail_data = string.Format("{0}^{1}^{2}",
               out_trade_no,
               amountToRefund,
               refundResult
             );
            //�˿�֪ͨ
            string notify_url = _webHelper.GetStoreLocation(false) + "Plugins/AliPay/RefundNotify";

            //�����˿��¼
            var refundInfo = new RefundInfo()
            {
                OrderId = refundPaymentRequest.Order.Id,
                Batch_no = batch_no,
                AmountToRefund = refundPaymentRequest.AmountToRefund,
                RefundStatusId = (int)RefundStatus.refunding,
                CreateOnUtc = DateTime.Now,
                Seller_Email = seller_emailToRefund,
                Seller_Id = seller_user_id,
                Out_Trade_No = out_trade_no,
            };
            _refundInfoService.Insert(refundInfo);

            ////////////////////////////////////////////////////////////////////////////////////////////////
            var alipayReturnConfig = new AlipayReturnConfig()
            {
                partner = _aliPayPaymentSettings.Partner,
                key = _aliPayPaymentSettings.Key,
                sign_type= "MD5",
                input_charset= "utf-8"
            };
            //������������������
            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            sParaTemp.Add("service", alipayReturnConfig.service);
            sParaTemp.Add("partner", alipayReturnConfig.partner);
            sParaTemp.Add("_input_charset", alipayReturnConfig.input_charset.ToLower());
            sParaTemp.Add("refund_date", alipayReturnConfig.refund_date);
            sParaTemp.Add("seller_user_id", seller_user_id);
            sParaTemp.Add("batch_no", batch_no);
            sParaTemp.Add("batch_num", batch_num);
            sParaTemp.Add("detail_data", detail_data);
            sParaTemp.Add("notify_url", notify_url);

            var post = AlipaySubmit.BuildRequest(sParaTemp, alipayReturnConfig, "POST");
            post.Post();

            result.AddError("�˿��������ύ,�뵽֧������վ�н����˿�ȷ��");//������,����Ӱ���˿���
            return result;
        }
        #endregion
        /// <summary>
        /// Voids a payment
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
        /// Process recurring payment
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
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();

            result.AddError("Recurring payment not supported");

            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //AliPay is the redirection payment method
            //It also validates whether order is also paid (after redirection) so customers will not be able to pay twice
            
            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;

            //let's ensure that at least 1 minute passed after order is placed
            return !((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1);
        }

        /// <summary>
        /// ����·��
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "AliPay";
            routeValues = new RouteValueDictionary { { "Namespaces", "DaBoLang.Nop.Plugin.Payments.AliPay.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// ֧����Ϣ·��
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "AliPay";
            routeValues = new RouteValueDictionary { { "Namespaces", "DaBoLang.Nop.Plugin.Payments.AliPay.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(AliPayController);
        }

        #region �����װ/ж��

        public override void Install()
        {
            //����
            var settings = new AliPayPaymentSettings
            {
                SellerEmail = "",
                Key = "",
                Partner = "",
                AdditionalFee = 0,
            };

            _settingService.SaveSetting(settings);

            //��װ���ݱ�
            _objectContext.Install();

            //���ػ���Դ
            this.AddOrUpdatePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.RedirectionTip", "�������ض���֧������վ��ɶ���.");
            this.AddOrUpdatePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.SellerEmail", "��������");
            this.AddOrUpdatePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.SellerEmail.Hint", "֧����������������.");
            this.AddOrUpdatePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.Key", "Key");
            this.AddOrUpdatePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.Key.Hint", "���� key.");
            this.AddOrUpdatePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.Partner", "Partner");
            this.AddOrUpdatePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.Partner.Hint", "���� partner.");
            this.AddOrUpdatePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.AdditionalFee", "�������");
            this.AddOrUpdatePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.AdditionalFee.Hint", "�ͻ�ѡ���֧����ʽ��������ķ���.");
            this.AddOrUpdatePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.PaymentMethodDescription", "ʹ��֧��������֧��");

            base.Install();
        }

        public override void Uninstall()
        {
            //���ػ���Դ
            this.DeletePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.SellerEmail.RedirectionTip");
            this.DeletePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.SellerEmail");
            this.DeletePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.SellerEmail.Hint");
            this.DeletePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.Key");
            this.DeletePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.Key.Hint");
            this.DeletePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.Partner");
            this.DeletePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.Partner.Hint");
            this.DeletePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.AdditionalFee");
            this.DeletePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("DaBoLang.Plugins.Payments.AliPay.PaymentMethodDescription");

            //ж�����ݱ�
            _objectContext.Uninstall();

            base.Uninstall();
        }

        #endregion
        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// ֧�ֲ����˿�
        /// true-֧��,false-��֧��
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// ֧���˿�
        /// true-֧��,false-��֧��
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription
        {
            get { return _localizationService.GetResource("DaBoLang.Plugins.Payments.AliPay.PaymentMethodDescription"); }
        }

        #endregion

    }
}
