using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore.Metadata;
using Persistence;

namespace Application.Activities
{
    public class Create
    {
        public class Command : IRequest
        {
            public Activity Activity { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext _context;
            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                _context.Activities.Add(request.Activity);//belum ke DB, EF mencatat di memori bahwa kita nambah data Activity

                await _context.SaveChangesAsync();

                return Unit.Value; //did nothing just return
            }

        }
    }
}