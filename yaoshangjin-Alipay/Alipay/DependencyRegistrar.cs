using Autofac;
using Autofac.Core;
using DaBoLang.Nop.Plugin.Payments.AliPay.Data;
using DaBoLang.Nop.Plugin.Payments.AliPay.Domain;
using DaBoLang.Nop.Plugin.Payments.AliPay.Services;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Web.Framework.Mvc;

namespace DaBoLang.Nop.Plugin.Payments.AliPay
{
    /// <summary>
    /// �����ռ䣺DaBoLang.Nop.Plugin.Payments.AliPay
    /// ��    �ƣ�DependencyRegistrar
    /// ��    �ܣ����
    /// ��    ϸ��ע��
    /// ��    ����1.0.0.0
    /// �ļ����ƣ�DependencyRegistrar.cs
    /// ����ʱ�䣺2017-08-02 01:03
    /// �޸�ʱ�䣺2017-08-04 01:52
    /// ��    �ߣ�����
    /// ��ϵ��ʽ��http://www.cnblogs.com/yaoshangjin
    /// ˵    ����
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            //data context
            this.RegisterPluginDataContext<AliPayObjectContext>(builder, "nop_object_context_alipay");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<PaymentInfo>>()
                .As<IRepository<PaymentInfo>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_alipay"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<RefundInfo>>()
               .As<IRepository<RefundInfo>>()
               .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_alipay"))
               .InstancePerLifetimeScope();
            //ע��֧����¼����
            builder.RegisterType<PaymentInfoService>().As<IPaymentInfoService>().InstancePerLifetimeScope();
            //ע���˿��¼����
            builder.RegisterType<RefundInfoService>().As<IRefundInfoService>().InstancePerLifetimeScope();
        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order
        {
            get { return 1; }
        }
    }
}
