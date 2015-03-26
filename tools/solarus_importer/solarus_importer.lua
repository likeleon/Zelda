#!/usr/bin/lua

-- Usage: ./solarus_importer.lua path/to/solarus/quest

local resource_list = require("resource_list")

if #arg ~= 1 then
  print("Usage: " .. arg[0] .. " path/to/solarus/quest")
  os.exit()
end

print("*** Importing solarus quest " .. arg[1] .. " ***")

local quest_path = arg[1] .. "/data/"

local resources = resource_list.check(quest_path)

print("*** Import completed. ***")
