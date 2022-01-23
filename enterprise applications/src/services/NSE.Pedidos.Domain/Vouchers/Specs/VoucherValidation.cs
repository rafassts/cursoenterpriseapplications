using NetDevPack.Specification;

namespace NSE.Pedidos.Domain.Specs
{
    //SpecValidator - classe do netdevpack
    public class VoucherValidation : SpecValidator<Voucher>
    {
        public VoucherValidation()
        {
            var dataSpec = new VoucherDataSpecification();
            var qtdeSpec = new VoucherQuantidadeSpecification();
            var ativoSpec = new VoucherAtivoSpecification();

            //especificação, mensagem
            Add("dataSpec", new Rule<Voucher>(dataSpec, "Este voucher está expirado"));
            Add("qtdeSpec", new Rule<Voucher>(qtdeSpec, "Este voucher já foi utilizado"));
            Add("ativoSpec", new Rule<Voucher>(ativoSpec, "Este voucher não está mais ativo"));
        }
    }
}