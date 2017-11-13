﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Api.Models.Storage;
using DealnetPortal.Domain;

namespace DealnetPortal.Api.Integration.Services.Signature
{

    public enum DocumentVersion
    {
        Draft = 0,
        Signed = 1
    };

    public interface ISignatureEngine
    {
        Task<IList<Alert>> ServiceLogin();

        Task<IList<Alert>> InitiateTransaction(Contract contract, AgreementTemplate agreementTemplate);

        Task<IList<Alert>> InsertDocumentFields(IList<FormField> formFields);

        Task<IList<Alert>> InsertSignatures(IList<SignatureUser> signatureUsers);

        Task<IList<Alert>> SubmitDocument(IList<SignatureUser> signatureUsers);

        Task<Tuple<IList<FormField>, IList<Alert>>> GetFormfFields();

        Task<Tuple<AgreementDocument, IList<Alert>>> GetDocument(DocumentVersion documentVersion);

        Task<IList<Alert>> CancelSignature();

        string TransactionId { get; set; }

        string DocumentId { get; set; }
    }
}
