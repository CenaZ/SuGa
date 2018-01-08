using Nop.Core.Configuration;

namespace DaBoLang.Nop.Plugin.Payments.AliPay
{
    /// <summary>
    /// �����ռ䣺DaBoLang.Nop.Plugin.Payments.AliPay
    /// ��    �ƣ�AliPayPaymentSettings
    /// ��    �ܣ�������
    /// ��    ϸ��֧��������
    /// ��    ����1.0.0.0
    /// �ļ����ƣ�AliPayPaymentSettings.cs
    /// ����ʱ�䣺2017-08-02 01:26
    /// �޸�ʱ�䣺2017-08-04 01:52
    /// ��    �ߣ�����
    /// ��ϵ��ʽ��http://www.cnblogs.com/yaoshangjin
    /// ˵    ����
    /// </summary>
    public class AliPayPaymentSettings : ISettings
    {
        /// <summary>
        /// ����Email
        /// </summary>
        public string SellerEmail { get; set; }
        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// PID
        /// </summary>
        public string Partner { get; set; }
        /// <summary>
        /// �������
        /// </summary>
        public decimal AdditionalFee { get; set; }
    }
}
