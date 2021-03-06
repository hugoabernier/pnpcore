﻿using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.Teams
{
    internal partial class TeamChatMessageCollection: QueryableDataModelCollection<ITeamChatMessage>, ITeamChatMessageCollection
    {
        public TeamChatMessageCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }

        public override ITeamChatMessage CreateNew()
        {
            return NewTeamChatMessage();
        }

        private TeamChatMessage AddNewTeamChatMessage()
        {
            var newTeamChatMessage = NewTeamChatMessage();
            this.items.Add(newTeamChatMessage);
            return newTeamChatMessage;
        }

        private TeamChatMessage NewTeamChatMessage()
        {
            var newTeamChatMessage = new TeamChatMessage
            {
                PnPContext = this.PnPContext,
                Parent = this,
            };
            return newTeamChatMessage;
        }
    }
}
