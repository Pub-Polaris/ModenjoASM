-- ModenjoASM bootstrap for the 7nnkmj port.
-- Keep this file tiny: the actual rules live in the C# assembly.
local function load_assemblies()
    local base = CS.UnityEngine.Application.streamingAssetsPath
    local dll = CS.System.IO.Path.Combine(base, "mods/7nnkmj_0.001/ModenjoASM.dll")
    CS.System.Reflection.Assembly.LoadFile(dll)
    CS.ModenjoASM.Bootstrap.Init()
end

function init()
    local ok, err = pcall(load_assemblies)
    if not ok then
        CS.Aotenjo.Logger.LogError("[7nnkmj] ModenjoASM load failed: " .. tostring(err))
    else
        CS.Aotenjo.Logger.Log("[7nnkmj] ModenjoASM loaded")
    end
end
