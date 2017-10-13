using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FdxPhoneCallEntity
{
    public class PhoneCall_Create : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            int step = 0;

            //Extract the tracing service for use in debugging sandboxed plug-ins....
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            //Obtain execution contest from the service provider....
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            //Call Input parameter collection to get all the data passes....
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity phonecallEntity = (Entity)context.InputParameters["Target"];
                Entity user = new Entity();

                if (phonecallEntity.LogicalName != "phonecall")
                    return;

                try
                {
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                    //Get current user information....
                    WhoAmIResponse response = (WhoAmIResponse)service.Execute(new WhoAmIRequest());

                    if(phonecallEntity.Attributes.Contains("from"))
                    {
                        EntityCollection fromCollection = (EntityCollection)phonecallEntity.Attributes["from"];
                        EntityReference userIdRef = new EntityReference();
                        foreach(var c in fromCollection.Entities)
                        {
                            if(c.Contains("partyid"))
                            {
                                userIdRef = (EntityReference)c.Attributes["partyid"];
                            }
                        }

                        if(userIdRef.LogicalName == "systemuser")
                        {
                            phonecallEntity.Attributes["fdx_caller"] = userIdRef;
                        }
                    }

                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException(string.Format("An error occurred in the PhoneCall_Create plug-in at Step {0}.", step), ex);
                }
                catch (Exception ex)
                {
                    tracingService.Trace("PhoneCall_Create: step {0}, {1}", step, ex.ToString());
                    throw;
                }
            }
        }
    }
}
