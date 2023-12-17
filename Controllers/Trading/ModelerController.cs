using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using TradingToolbox.Trading.Modeler.Data.NoSql.MongoDB.Models;
using TradingToolbox.Trading.Modeler.Data.NoSql.MongoDB.Services.PositionModeling;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TradingToolbox.Applications.Trading.Modeler.ServiceApp.Controllers
{
    // TODO move patch classes to a more general project assembly.

    /// <summary>
    /// Base patch request for field level changes/patches on a model.
    /// </summary>
    /// <typeparam name="T">The type of model to apply field level changes/patches on.</typeparam>
    public abstract class BasePatchRequest<T> where T : class
    {
        public JsonPatchDocument<T> patchDocument { get; set; }
    }

    /// <summary>
    /// Patch request with list of string IDs as the document identifiers used to look up the documents to apply field level changes/patchs on.
    /// </summary>
    /// <typeparam name="T">The type of model to apply field level changes/patches on.</typeparam>
    /// <seealso cref="TradingToolbox.Applications.Trading.Modeler.ServiceApp.Controllers.BasePatchRequest{T}" />
    public sealed class PatchRequest<T> : BasePatchRequest<T> where T : class
    {
        public string[] ids { get; set; }
    }

    /// <summary>
    /// Controller for trading position modeling.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [ApiController]
    [Route("api/teqbench/tradingtoolbox/trading/[controller]")]
    public class ModelerController : ControllerBase
    {
        private readonly IMongoDbService _mongoDbService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelerController" /> class.
        /// </summary>
        /// <param name="mongoDbService">The mongo database service to do DB operstaions.</param>
        public ModelerController(IMongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        /// <summary>
        /// Gets all of the position documents from the DB
        /// </summary>
        /// <returns>List of position documents.</returns>
        [HttpGet("positions")]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<PositionModelDocument> items = new List<PositionModelDocument>();

            try
            {
                // For now, just force sort to be by ListPosition and do here...can move to FindAsync later...
                // TODO - Add sort capability to FindAsync in TeqBench.System.Data.NoSql.MongoDb.Repository
                items = (await _mongoDbService.PositionModelRepository.FindAsync(_ => true)).OrderBy(item => item.ListPosition);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }

            return Ok(items);
        }

        /// <summary>
        /// Adds one position document to the DB
        /// </summary>
        /// <param name="value">The position document to add.</param>
        /// <returns>An instance of ObjectResult with the appropriate HTTP status code and value of updated document with ID assigned from DB if successful.</returns>
        [HttpPost("position")]
        public async Task<IActionResult> AddOne(PositionModelDocument value)
        {
            try
            {
                // Find the last model and use it to set the ListPosition of the new model being added.
                PositionModelDocument item = (await _mongoDbService.PositionModelRepository.FindAsync(_ => true))
                    .OrderBy(item => item.ListPosition)
                    .LastOrDefault();

                value.ListPosition = (item == null) ? 0 : item.ListPosition + 1;

                // Just return 'value' as OK status if no errors.
                await _mongoDbService.PositionModelRepository.InsertOneAsync(value);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }

            return Ok(value);
        }

        /// <summary>
        /// Updates supplied position document in the DB.
        /// </summary>
        /// <param name="value">The position document to update in the DB.</param>
        /// <returns>An instance of ObjectResult with the appropriate HTTP status code and value of updated document if successful.</returns>
        [HttpPut("position")]
        public async Task<IActionResult> Update(PositionModelDocument value)
        {
            try
            {
                await _mongoDbService.PositionModelRepository.ReplaceOneAsync(value);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }

            return Ok(value);
        }

// BV 12/16/23 Appears to be unused, comment out for now.
// TODO: Eval if actually need this Update method.
//        /// <summary>
//        /// Updates a single document with field level changes (patches).
//        /// </summary>
//        /// <param name="patchRequest">The patch request to update one position documents with the field level changes/patches.</param>
//        /// <returns>An instance of ObjectResult with the appropriate HTTP status code and list of updated documents.</returns>
//        [HttpPatch("position")]
//        public async Task<IActionResult> Update([FromBody] PatchRequest<PositionModelDocument> patchRequest)
//        {
//            // NOTE: In nearly all examples, patch documents are passed as part of the body and any ID is part of the resource URL.
//            // In this case with more than one ID, necessary to pass both as part of the body; tried to pass one in the header
//            // and the other as part of the body, but was having a heck of a time getting the data out of out of the header on 
//            // the serve side; worked OK when request came from Swagger but from Angular, could not get it to work. Compromised 
//            // solution is current implementation of sending both as part of the body.

//            // TODO: Add transactional support to all DB operations
//            // this really should be transactional, but there's some sort of issues w/ running mongo locally (maybe has to do with docker)

//            // Do some safety checking on the request param before attemping to process it.
//            if ((patchRequest == null) ||
//                (patchRequest.ids == null) ||
//                (patchRequest.ids.Count() < 1) ||
//                (patchRequest.patchDocument == null))
//            {
//                return StatusCode(StatusCodes.Status400BadRequest);
//            }

//            List<PositionModelDocument> values = new List<PositionModelDocument>();
            
//            try
//            {
//#if DEBUG
//                Stopwatch stopwatch = new Stopwatch();
//                stopwatch.Start();
//#endif
//                values = await processPatchRequst(patchRequest);
//#if DEBUG
//                stopwatch.Stop();
//                Console.WriteLine("Elapsed Time: {0} ms", stopwatch.ElapsedMilliseconds);
//#endif
//            }
//            catch (Exception e)
//            {
//                return StatusCode(StatusCodes.Status500InternalServerError, e);
//            }

//            // May change to ONLY return Ok() ... not sure yet of the pros/cons of returning the new values or not, or if even needed...keep for now.
//            return Ok(values);
//        }

        /// <summary>
        /// Updates multiple documents with field level changes (patches).
        /// </summary>
        /// <param name="patchRequests">The patch requests to update one or more position documents with the field level changes/patches.</param>
        /// <returns>An instance of ObjectResult with the appropriate HTTP status code and list of updated documents.</returns>
        [HttpPatch("positions")]
        public async Task<IActionResult> Update([FromBody] PatchRequest<PositionModelDocument>[] patchRequests)
        {
            // NOTE: In nearly all examples, patch documents are passed as part of the body and any ID is part of the resource URL.
            // In this case with more than one ID, necessary to pass both as part of the body; tried to pass one in the header
            // and the other as part of the body, but was having a heck of a time getting the data out of out of the header on 
            // the serve side; worked OK when request came from Swagger but from Angular, could not get it to work. Compromised 
            // solution is current implementation of sending both as part of the body.

            // TODO: Add transactional support to all DB operations
            // this really should be transactional, but there's some sort of issues w/ running mongo locally (maybe has to do with docker)

            // First, check ALL the request are valid before even trying to update in the DB
            foreach (PatchRequest<PositionModelDocument> patchRequest in patchRequests)
            {
                // Do some safety checking on the request param before attemping to process it.
                if ((patchRequest == null) ||
                (patchRequest.ids == null) ||
                (patchRequest.ids.Count() < 1) ||
                (patchRequest.patchDocument == null))
                {
                    return StatusCode(StatusCodes.Status400BadRequest);
                }
            }

            List<PositionModelDocument> values = new List<PositionModelDocument>();

            // Second, if all OK, then process each request and accumulate return values
            foreach (PatchRequest<PositionModelDocument> patchRequest in patchRequests)
            {
                try
                {
#if DEBUG
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
#endif
                    values.AddRange(await processPatchRequst(patchRequest));
#if DEBUG
                    stopwatch.Stop();
                    Console.WriteLine("Elapsed Time: {0} ms", stopwatch.ElapsedMilliseconds);
#endif
                }
                catch (Exception e)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, e);
                }
            }

            // May change to ONLY return Ok() ... not sure yet of the pros/cons of returning the new values or not, or if even needed...keep for now.
            return Ok(values);
        }

        /// <summary>
        /// Processes a single patch requst for the associated position documents IDs.
        /// </summary>
        /// <param name="patchRequest">The patch request with the IDs of the position documents to update and the field level changes/pathces to apply.</param>
        /// <returns>List of updated docucments.</returns>
        private async Task<List<PositionModelDocument>> processPatchRequst(PatchRequest<PositionModelDocument> patchRequest)
        {
            List<PositionModelDocument> values = new List<PositionModelDocument>();

            try
            {
                // Wondering if this would be a candidate for parallelism
                foreach (string id in patchRequest.ids)
                {
                    PositionModelDocument model = await _mongoDbService.PositionModelRepository.FindByIdAsync(id);

                    patchRequest.patchDocument.ApplyTo(model);

                    await _mongoDbService.PositionModelRepository.ReplaceOneAsync(model);

                    values.Add(model);
                }
            }
            catch (Exception e)
            {
                throw;
            }

            return values;
        }

        /// <summary>
        /// Deletes a position document by ID.
        /// </summary>
        /// <param name="id">The ID used to lookup the position document to delete.</param>
        /// <returns>An instance of ObjectResult with the appropriate HTTP status code.</returns>
        [HttpDelete("position/{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            try
            {
                await _mongoDbService.PositionModelRepository.DeleteByIdAsync(id);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }

            return Ok();
        }

        /// <summary>
        /// Deletes multiple position documents as specified the specified list of IDs.
        /// </summary>
        /// <param name="ids">The IDs of position documents used to lookup the documents to delete.</param>
        /// <returns>An instance of ObjectResult with the appropriate HTTP status code.</returns>
        [HttpPost("positions/delete")]
        public async Task<IActionResult> Delete([FromBody] string[] ids)
        {
            try
            {
                // from https://gist.github.com/Kusken/3c2f9b764b0adf18d09761427f1ba0ee
                await _mongoDbService.PositionModelRepository.DeleteManyAsync(doc => ids.Contains(doc.Id));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }

            return Ok();
        }
    }
}
