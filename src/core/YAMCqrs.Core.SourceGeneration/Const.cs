namespace YAMCqrs.Core.SourceGeneration;

public static class Const
{
    public const string NamespaceName = "namespace YAMCqrs.Core;";
    public static class InterfaceNames
    {
        public const string Command = "YAMCqrs.Core.Abstractions.Commands.ICommand";
        public const string CommandHandler = "YAMCqrs.Core.Abstractions.Commands.ICommandHandler<TCommand, TResult>";
        public const string CommandInterceptor = "YAMCqrs.Core.Abstractions.Commands.ICommandInterceptor<TCommand, TResult>";

        public const string Query = "YAMCqrs.Core.Abstractions.Queries.IQuery";
        public const string QueryHandler = "YAMCqrs.Core.Abstractions.Queries.IQueryHandler<TQuery, TResult>";
        public const string QueryInterceptor = "YAMCqrs.Core.Abstractions.Queries.IQueryInterceptor<TQuery, TResult>";
    }

    public static class Usings
    {
        public const string ICommand = "using YAMCqrs.Core.Abstractions.Commands;";
        public const string IQuery = "using YAMCqrs.Core.Abstractions.Queries;";
        public const string IDispatcher = "using YAMCqrs.Core.Abstractions;";
    }
}