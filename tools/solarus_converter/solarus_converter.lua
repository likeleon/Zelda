#!/usr/bin/lua

-- Usage: ./solarus_converter.lua path/to/solarus/quest

local report = require("report")
local resource_list = require("resource_list")
local tileset_converter = require("tileset_converter")

if #arg ~= 1 then
  print("Usage: " .. arg[0] .. " path/to/solarus/quest")
  os.exit()
end

print("*** Converting solarus quest " .. arg[1] .. " ***")

local quest_path = arg[1] .. "/data/"

local resources = resource_list.check(quest_path)
tileset_converter.convert(quest_path, resources)

print("*** Convert completed with " .. report.get_num_warnings() ..
  " warning(s) and " .. report.get_num_errors() .. " error(s). ***")
