using System;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Registrants
{
    public class CommandsHandler
    {
        private readonly IRepository<Registration> repository;

        public CommandsHandler(IRepository<Registration> repository)
        {
            this.repository = repository;
        }

        public async Task<Guid> Handle(RegisterNew cmd)
        {
            var registration = new Registration(cmd.Name, cmd.Address, cmd.DateOfBirth);
            await repository.SaveAsync(registration);
            return registration.Id;
        }

        public async Task Handle(UpdateDetails cmd)
        {
            var registration = await repository.GetByIdAsync(cmd.Id);
            registration.UpdateAddress(cmd.Address);
            registration.UpdateName(cmd.Name);
            await repository.SaveAsync(registration);
        }

        public async Task Handle(AddIdentifier cmd)
        {
            var registration = await repository.GetByIdAsync(cmd.Id);
            registration.AddIdentifier(cmd.IdentifierType, cmd.IdentifierValue);
            await repository.SaveAsync(registration);
        }

        public async Task Handle(ReplaceIdentifier cmd)
        {
            var registration = await repository.GetByIdAsync(cmd.Id);
            registration.ReplaceIdentifier(cmd.IdentifierType, cmd.IdentifierNewValue);
            await repository.SaveAsync(registration);
        }

        public async Task Handle(RemoveIdentifier cmd)
        {
            var registration = await repository.GetByIdAsync(cmd.Id);
            registration.RemoveIdentifier(cmd.IdentifierType);
            await repository.SaveAsync(registration);
        }
    }
}
