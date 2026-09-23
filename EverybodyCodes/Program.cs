using EverybodyCodes;

using EverybodyCodesSetup setup = new();
if (!await setup.TrySetup()) return 1;

return await setup.RunProgram(args);
