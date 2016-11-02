using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess.Repositories
{
    public interface IFileRepository
    {
        AgreementTemplate AddOrUpdateAgreementTemplate(AgreementTemplate agreementTemplate);

        AgreementTemplate FindAgreementTemplate(Expression<Func<AgreementTemplate, bool>> predicate);

        IList<AgreementTemplate> FindAgreementTemplates(Expression<Func<AgreementTemplate, bool>> predicate);

        AgreementTemplate GetAgreementTemplate(int templateId);

        AgreementTemplate RemoveAgreementTemplate(AgreementTemplate agreementTemplate);
    }
}
