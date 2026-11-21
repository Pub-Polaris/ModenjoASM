-- ModenjoASM bootstrap for the Aotenjo extension host.
-- Keep this file tiny: the loader lives in the C# assembly and module DLLs.
local function load_assemblies()
    local base = CS.UnityEngine.Application.streamingAssetsPath
    local modDir = CS.System.IO.Path.Combine(base, "mods/7nnkmj_0.001")
    local dll = CS.System.IO.Path.Combine(modDir, "ModenjoASM.dll")
    local asm = CS.System.Reflection.Assembly.LoadFile(dll)
    CS.Aotenjo.Logger.Log("[ModenjoASM] host assembly: " .. tostring(asm and asm.FullName))

    -- xLua's CS namespace cache may not see a type from an assembly loaded after
    -- startup. Resolve it explicitly through System.Type.
    local t = CS.System.Type.GetType("ModenjoASM.Bootstrap, ModenjoASM")
    if t == nil then
        error("Bootstrap type not found")
    end

    local runner = asm:CreateInstance("ModenjoASM.Bootstrap")
    if runner == nil then
        error("Bootstrap instance not found")
    end

    runner:Init(modDir)
end

function init()
    local ok, err = pcall(load_assemblies)
    if not ok then
        CS.Aotenjo.Logger.LogError("[ModenjoASM] load failed: " .. tostring(err))
    else
        CS.Aotenjo.Logger.Log("[ModenjoASM] host loaded")
    end
end
