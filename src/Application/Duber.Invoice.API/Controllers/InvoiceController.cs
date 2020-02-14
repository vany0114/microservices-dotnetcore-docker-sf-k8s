using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Duber.Domain.Invoice.Repository;
using Duber.Domain.Invoice.Services;
using Duber.Domain.SharedKernel.Model;
using Microsoft.AspNetCore.Mvc;
using ViewModel = Duber.Invoice.API.Application.Model;

namespace Duber.Invoice.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class InvoiceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IPaymentService _paymentService;
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceController(IInvoiceRepository invoiceRepository, IMapper mapper, IPaymentService paymentService)
        {
            _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        }

        /// <summary>
        /// Returns an invoice that matches with the specified id
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <returns>Returns an invoice that matches with the specified id</returns>
        /// <response code="200">Returns an Invoice object that matches with the specified id</response>
        [Route("getbyid")]
        [HttpGet]
        [ProducesResponseType(typeof(ViewModel.Invoice), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetInvoice(Guid invoiceId)
        {
            try
            {
                var invoice = await _invoiceRepository.GetInvoiceAsync(invoiceId);
                var invoiceViewModel = _mapper.Map<ViewModel.Invoice>(invoice);

                if (invoiceViewModel == null)
                    return NotFound();

                return Ok(invoiceViewModel);
            }
            finally
            {
                _invoiceRepository.Dispose();
            }
        }

        /// <summary>
        /// Returns an invoice that matches with the specified trip id
        /// </summary>
        /// <param name="tripId"></param>
        /// <returns>Returns an invoice that matches with the specified trip id</returns>
        /// <response code="200">Returns an invoice that matches with the specified trip id</response>
        [Route("getbytripid")]
        [HttpGet]
        [ProducesResponseType(typeof(ViewModel.Invoice), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetInvoiceByTrip(Guid tripId)
        {
            try
            {
                var invoice = await _invoiceRepository.GetInvoiceByTripAsync(tripId);
                var invoiceViewModel = _mapper.Map<ViewModel.Invoice>(invoice);

                if (invoiceViewModel == null)
                    return NotFound();

                return Ok(invoiceViewModel);
            }
            finally
            {
                _invoiceRepository.Dispose();
            }
        }

        /// <summary>
        /// Returns all of the Invoices
        /// </summary>
        /// <returns>Returns all of the Invoices</returns>
        /// <response code="200">Returns a list of Invoice object.</response>
        [Route("get")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ViewModel.Invoice>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetInvoices()
        {
            try
            {
                var invoices = await _invoiceRepository.GetInvoicesAsync();
                var invoicesViewModel = _mapper.Map<IEnumerable<ViewModel.Invoice>>(invoices);

                if (invoicesViewModel == null)
                    return NotFound();

                return Ok(invoicesViewModel);
            }
            finally
            {
                _invoiceRepository.Dispose();
            }
        }

        /// <summary>
        /// Creates a new invoice.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Returns the newly created invoice identifier.</returns>
        /// <response code="201">Returns the newly created trip identifier.</response>
        [Route("create")]
        [HttpPost]
        [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateInvoice([FromBody]ViewModel.CreateInvoiceRequest request)
        {
            try
            {
                // to enable idempotency.
                var invoice = await _invoiceRepository.GetInvoiceByTripAsync(request.TripId);
                if (invoice != null) return CreatedAtAction(nameof(GetInvoice), new { invoiceId = invoice.InvoiceId }, invoice.InvoiceId);

                invoice = new Domain.Invoice.Model.Invoice(
                    request.PaymentMethod.Id,
                    request.TripId,
                    request.Duration,
                    request.Distance,
                    request.TripStatus.Id);

                await _invoiceRepository.AddInvoiceAsync(invoice);

                // integration with external payment system.
                if (Equals(invoice.PaymentMethod, PaymentMethod.CreditCard) && invoice.Total > 0)
                {
                    await _paymentService.PerformPayment(invoice, request.UserId);
                }

                return CreatedAtAction(nameof(GetInvoice), new { invoiceId = invoice.InvoiceId }, invoice.InvoiceId);
            }
            finally
            {
                _invoiceRepository.Dispose();
            }
        }
    }
}