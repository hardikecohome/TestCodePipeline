﻿using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Domain;

namespace DealnetPortal.Api.Integration.Services.Signature
{
    public interface ISignatureEngine
    {
        Task<IList<Alert>> ServiceLogin();

        Task<IList<Alert>> StartNewTransaction(Contract contract);

        Task<IList<Alert>> InsertDocumentFields(IList<FormField> formFields);

        Task<IList<Alert>> InsertSignatures(IList<SignatureUser> signatureUsers);

        Task<IList<Alert>> SendInvitations();

        string TransactionId { get; }

        string DocumentId { get; }
    }
}
