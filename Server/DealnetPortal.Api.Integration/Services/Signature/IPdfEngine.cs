using System;
using System.Collections.Generic;
using System.IO;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Signature;

namespace DealnetPortal.Api.Integration.Services.Signature
{
    public interface IPdfEngine
    {
        Tuple<Stream, IList<Alert>> InsertFormFields(Stream inFormStream, IList<FormField> formFields);

        Tuple<IList<FormField>, IList<Alert>> GetFormfFields(Stream inFormStream);
    }
}
