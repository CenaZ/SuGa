using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace DaBoLang.Nop.Plugin.Payments.AliPay.Domain
{
    /// <summary>
    /// �����ռ䣺DaBoLang.Nop.Plugin.Payments.AliPay.Domain
    /// ��    �ƣ�RefundInfo
    /// ��    �ܣ�ʵ����
    /// ��    ϸ���˿��¼
    /// ��    ����1.0.0.0
    /// ����ʱ�䣺2017-08-03 05:34
    /// �޸�ʱ�䣺2017-08-04 01:26
    /// �޸�ʱ�䣺time
    /// ��    �ߣ�����
    /// ��ϵ��ʽ��http://www.cnblogs.com/yaoshangjin
    /// ˵    ����
    /// </summary>
    public partial class RefundInfo : BaseEntity
    {

        #region Properties
        public int OrderId { get; set; }
        /// <summary>
        /// �˿�״̬
        /// </summary>
        public int RefundStatusId { get; set; }
        /// <summary>
        /// �˿���
        /// </summary>
        public decimal AmountToRefund { get; set; }
        public string Seller_Email { get; set; }
        public string Seller_Id { get; set; }
        /// <summary>
        /// ���׺ţ��ڲ����׺ţ�֧�������׺Ż���΢�Ž��׺�
        /// </summary>
        public string Batch_no { get; set; }
        /// <summary>
        /// ��������ⲿ���׺�
        /// </summary>
        public string Out_Trade_No { get; set; }
        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime CreateOnUtc { get; set; }
        /// <summary>
        /// �˿�ɹ�ʱ��
        /// </summary>
        public DateTime? RefundOnUtc { get; set; }
        
        /// <summary>
        /// �ص�ID
        /// </summary>
        public string Notify_Id { get; set; }
        /// <summary>
        /// �ص�����
        /// </summary>
        public string Notify_Type { get; set; }

        public string Result_Details { get; set; }
        #endregion

        /// <summary>
        /// ����״̬
        /// </summary>
        public RefundStatus RefundStatus
        {
            get
            {
                return (RefundStatus)this.RefundStatusId;
            }
            set
            {
                this.RefundStatusId = (int)value;
            }
        }

    }
    public enum RefundStatus
    {
        /// <summary>
        /// �����˿�
        /// </summary>
        refunding = 10,
        /// <summary>
        /// �˿�ɹ�
        /// </summary>
        refund = 20,
        /// <summary>
        /// ȡ���˿�
        /// </summary>
        cancel = 30,
        /// <summary>
        /// �˿����
        /// </summary>
        overtime = 40,
        /// <summary>
        /// �˿�ʧ��
        /// </summary>
        error = 50,
    }
}
