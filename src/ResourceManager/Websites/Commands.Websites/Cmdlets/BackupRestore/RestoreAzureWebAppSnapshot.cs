﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.Azure.Commands.WebApps.Utilities;
using Microsoft.Azure.Management.WebSites.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Commands.WebApps.Cmdlets.BackupRestore
{
    /// <summary>
    ///     Restores an Azure Web App snapshot
    /// </summary>
    [Cmdlet(VerbsData.Restore, "AzureRmWebAppSnapshot")]
    public class RestoreAzureWebAppSnapshot : WebAppOptionalSlotBaseCmdlet
    {
        [Parameter(Position = 3, Mandatory = true, HelpMessage = "The timestamp of the snapshot.", ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string SnapshotTime;

        [Parameter(Mandatory = false, HelpMessage = "Recover the web app's configuration in addition to files.", ValueFromPipelineByPropertyName = true)]
        public SwitchParameter RecoverConfiguration { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The app that the snapshot contents will be restored to. Must be a slot of the original app. If unspecified, the original app is overwritten.", ValueFromPipelineByPropertyName = true)]
        public Site TargetApp { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Restore the snapshot without displaying warning about possible data loss.", ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Force { get; set; }

        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();
            SnapshotRecoveryTarget target = null;
            if (this.TargetApp != null)
            {
                string webAppName, slotName;
                CmdletHelpers.TryParseAppAndSlotNames(Name, out webAppName, out slotName);
                if (!string.Equals(this.TargetApp.ResourceGroup, this.ResourceGroupName, StringComparison.InvariantCultureIgnoreCase) ||
                    !string.Equals(webAppName, this.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new PSArgumentException("Target app must be a slot of the source web app");
                }
                target = new SnapshotRecoveryTarget()
                {
                    Location = TargetApp.Location,
                    Id = TargetApp.Id
                };
            }
            SnapshotRecoveryRequest recoveryReq = new SnapshotRecoveryRequest()
            {
                Overwrite = true,
                SnapshotTime = this.SnapshotTime,
                RecoverConfiguration = this.RecoverConfiguration,
                IgnoreConflictingHostNames = true,
                RecoveryTarget = target
            };
            Action recoverAction = () => WebsitesClient.RecoverSite(ResourceGroupName, Name, Slot, recoveryReq);
            ConfirmAction(this.Force.IsPresent, "Web app contents will be overwritten with the contents of the snapshot.",
                "The snapshot has been restored.", Name, recoverAction);
        }
    }
}
