using EverybodyCodes.Resolver;

using EverybodyCodesSetup solverSetup = new();
if (!await solverSetup.TrySetup()) return 1;

return await solverSetup.RunProgram(args);
