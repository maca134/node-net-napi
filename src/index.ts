type LoadClr = <T>(assemblyPath: string, typeName: string, methodName: string) => T;

export const loadClr = require('bindings')('net-napi.node') as LoadClr;