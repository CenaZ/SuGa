using DaBoLang.Nop.Plugin.Payments.AliPay.Domain;
using Nop.Data.Mapping;

namespace DaBoLang.Nop.Plugin.Payments.AliPay.Data
{
    /// <summary>
    /// �����ռ䣺DaBoLang.Nop.Plugin.Payments.AliPay.Data
    /// ��    �ƣ�RefundInfoMap
    /// ��    �ܣ�ʵ��ӳ��
    /// ��    ϸ���˿��¼ӳ��
    /// ��    ����1.0.0.0
    /// �ļ����ƣ�RefundInfoMap.cs
    /// ����ʱ�䣺2017-08-03 05:39
    /// �޸�ʱ�䣺2017-08-04 01:31
    /// ��    �ߣ�����
    /// ��ϵ��ʽ��http://www.cnblogs.com/yaoshangjin
    /// ˵    ����
    /// </summary>
    public partial class RefundInfoMap : NopEntityTypeConfiguration<RefundInfo>
    {
        public RefundInfoMap()
        {
            this.ToTable("dbl_RefundInfo");
            this.HasKey(x => x.Id);
            this.Ignore(x=>x.RefundStatus);
        }
    }
}