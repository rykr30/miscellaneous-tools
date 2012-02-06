#!/usr/bin/env ruby

require 'rubygems'
require 'popen4'
require 'find'

def usage(message = "")

  unless message.empty?
    puts
    puts "ERROR: #{message}"
    puts
  end

  puts "Usage:"
  puts
  puts "  svnall.rb folder subversion command parameters."
  puts
  puts "Examples:"
  puts
  puts "  svnall.rb /data/repos info"
  puts "  svnall.rb /data/repos update"
  puts "  svnall.rb /data/repos commit"
  puts

  exit 1

end

usage "At least 2 arguments required" if ARGV.count < 2

target_directory = ARGV.shift

usage("\"#{target}\" must be a directory and must exist.") unless File.directory? target_directory

subversion_working_copies = Array.new

Find.find(File.absolute_path(target_directory)) do |path|
  if FileTest.directory?(path) && File.basename(path) =~ /\.svn$/

    parent_folder = File.dirname path

    add_this_directory = true

    subversion_working_copies.each { |dir| add_this_directory = parent_folder.scan(dir).count == 0 }

    subversion_working_copies << parent_folder if add_this_directory

    Find.prune # Don't look any further into this directory.
  else
    next
  end
end

subversion_working_copies.each do |dir|

  puts "====>>> BEGIN Processing: #{dir} ========"

  command = "svn #{ARGV.join " "} #{dir}"

  pid, stdin, stdout, stderr = Open4::popen4 command

  puts "# pid: #{pid}"
  puts "# command: #{command}"
  puts
  
  ignored, status = Process::waitpid2 pid

  puts "#{ stdout.read.strip }"
  puts "#{ stderr.read.strip }"
  puts "exitstatus: #{ status.exitstatus }"

  puts "====<<< END Processing: #{dir} ========"
  puts

end
