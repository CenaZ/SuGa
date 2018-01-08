using DaBoLang.Nop.Plugin.Payments.AliPay.Domain;
using Nop.Data.Mapping;

namespace DaBoLang.Nop.Plugin.Payments.AliPay.Data
{
    /// <summary>
    /// �����ռ䣺DaBoLang.Nop.Plugin.Payments.AliPay.Data
    /// ��    �ƣ�PaymentInfoMap
    /// ��    �ܣ�ʵ��ӳ��
    /// ��    ϸ��֧����ӳ��
    /// ��    ����1.0.0.0
    /// �ļ����ƣ�PaymentInfoMap.cs
    /// ����ʱ�䣺2017-08-03 12:05
    /// �޸�ʱ�䣺2017-08-04 01:30
    /// ��    �ߣ�����
    /// ��ϵ��ʽ��http://www.cnblogs.com/yaoshangjin
    /// ˵    ����
    /// </summary>
    public partial class PaymentInfoMap : NopEntityTypeConfiguration<PaymentInfo>
    {
        public PaymentInfoMap()
        {
            this.ToTable("dbl_PaymentInfo");
            this.HasKey(x => x.Id);
        }
    }
}