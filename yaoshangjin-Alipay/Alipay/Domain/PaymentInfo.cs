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
    /// ��    �ƣ�PaymentInfo
    /// ��    �ܣ�ʵ����
    /// ��    ϸ�������¼
    /// ��    ����1.0.0.0
    /// ����ʱ�䣺2017-08-03 12:30
    /// �޸�ʱ�䣺2017-08-04 01:26
    /// �޸�ʱ�䣺time
    /// ��    �ߣ�����
    /// ��ϵ��ʽ��http://www.cnblogs.com/yaoshangjin
    /// ˵    ����
    /// </summary>
    public partial class PaymentInfo : BaseEntity
    {
        #region Properties
        public Guid PaymentGuid { get; set; }
        /// <summary>
        /// �������
        /// </summary>
        public int OrderId { get; set; }
        /// <summary>
        /// ���SystemName
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// ���׽��
        /// </summary>
        public decimal Total { get; set; }
        /// <summary>
        /// ��������ⲿ���׺�
        /// </summary>
        public string Out_Trade_No { get; set; }
        /// <summary>
        /// ˵��
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// ���׺ţ��ڲ����׺ţ�֧�������׺Ż���΢�Ž��׺�
        /// </summary>
        public string Trade_no { get; set; }
        /// <summary>
        /// ����������״̬
        /// </summary>
        public string Trade_status { get; set; }
        /// <summary>
        /// �տλemail
        /// </summary>
        public string Seller_email { get; set; }
        /// <summary>
        /// �տλid
        /// </summary>
        public string Seller_id { get; set; }
        /// <summary>
        /// �����˻�id
        /// </summary>
        public string Buyer_id { get; set; }
        /// <summary>
        /// �����˻�email
        /// </summary>
        public string Buyer_email { get; set; }      
       /// <summary>
       /// �ڲ���������ʱ��
       /// </summary>
        public DateTime CreateDateUtc { get; set; }

        #endregion



    }
}
