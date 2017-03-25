COMPILE_TARGET = ENV['config'].nil? ? "debug" : ENV['config']
BUILD_VERSION = '100.0.0'
tc_build_number = ENV["BUILD_NUMBER"]
build_revision = tc_build_number || Time.new.strftime('5%H%M')
build_number = "#{BUILD_VERSION}.#{build_revision}"
BUILD_NUMBER = build_number 
msbuild = '"C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe"'

desc 'Compile the code'
task :compile => [:clean, :version] do
  sh "#{msbuild} src/Bottles.Console/Bottles.Console.csproj /property:Configuration=#{COMPILE_TARGET} /v:m /t:rebuild /nr:False /maxcpucount:8"
  sh "src/Bottles.Console/bin/#{COMPILE_TARGET}/BottleRunner.exe assembly-pak src/AssemblyPackage"
  sh "#{msbuild} src/Bottles.sln /property:Configuration=#{COMPILE_TARGET} /v:m /t:rebuild /nr:False /maxcpucount:8"
end

desc "Compiles the BottleProject"
task :compile_bottle_project do
  sh "#{msbuild} bottles-staging/BottleProject.csproj /property:Configuration=#{COMPILE_TARGET} /v:m /t:rebuild /nr:False /maxcpucount:8"
end

desc "Update the version information for the build"
task :version do
  asm_version = build_number
  
  begin
    commit = `git log -1 --pretty=format:%H`
  rescue
    commit = "git unavailable"
  end
  puts "##teamcity[buildNumber '#{build_number}']" unless tc_build_number.nil?
  puts "Version: #{build_number}" if tc_build_number.nil?
  
  options = {
    :description => 'Bottles',
    :product_name => 'Bottles',
    :copyright => 'Copyright 2010-2011 Jeremy D. Miller, Dru Sellers, et al. All rights reserved.',
    :trademark => commit,
    :version => asm_version,
    :file_version => build_number,
    :informational_version => asm_version
  }
  
  puts "Writing src/CommonAssemblyInfo.cs..."
  File.open('src/CommonAssemblyInfo.cs', 'w') do |file|
    file.write "using System.Reflection;\n"
    file.write "using System.Runtime.InteropServices;\n"
    file.write "[assembly: AssemblyDescription(\"#{options[:description]}\")]\n"
    file.write "[assembly: AssemblyProduct(\"#{options[:product_name]}\")]\n"
    file.write "[assembly: AssemblyCopyright(\"#{options[:copyright]}\")]\n"
    file.write "[assembly: AssemblyTrademark(\"#{options[:trademark]}\")]\n"
    file.write "[assembly: AssemblyVersion(\"#{options[:version]}\")]\n"
    file.write "[assembly: AssemblyFileVersion(\"#{options[:file_version]}\")]\n"
    file.write "[assembly: AssemblyInformationalVersion(\"#{options[:informational_version]}\")]\n"
  end
end

desc "Prepares the working directory for a new build"
task :clean do
  FileUtils.rm_rf 'artifacts'
  Dir.mkdir 'artifacts'
end

desc 'Run the unit tests'
task :test => [:compile, :fast_test] do
end

desc 'Run the integration tests'
task :integration_test => [:compile, :fast_integration_test] do
end

desc 'Run the unit tests without compile'
task :fast_test do
  sh "src/packages/NUnit.ConsoleRunner.3.6.1/tools/nunit3-console.exe src/Bottles.Tests/bin/#{COMPILE_TARGET}/Bottles.Tests.dll"
end

desc 'Run the integration tests without compile'
task :fast_integration_test do
  sh "src/packages/NUnit.ConsoleRunner.3.6.1/tools/nunit3-console.exe src/Bottles.IntegrationTesting/bin/#{COMPILE_TARGET}/Bottles.IntegrationTesting.dll"
end

#desc "**Default**, compiles, merges and runs tests"
#task :default => [:compile, :ilrepack, :unit_test, :integration_test]
#desc "**Mono**, compiles, merges and runs unit tests"
#task :mono_ci => [:compile, :ilrepack, :unit_test]
#desc "Target used for the CI server"
#task :ci => [:compile, :ilrepack, :unit_test,:history, :package]

desc "Merge dotnetzip assembly into Bottles projects"
task :ilrepack do
  merge_ionic("src/Bottles/bin/#{COMPILE_TARGET}", 'Bottles.dll')
  #merge_fubucsprojfile("src/Bottles/bin/#{COMPILE_TARGET}", 'Bottles.dll')
end

def merge_ionic(dir, assembly)
	output = File.join(dir, assembly)
  sh "tools/ILMerge.exe /ndebug /target:library /targetplatform:v4 /internalize /out:#{output} /lib:#{dir} #{assembly} Ionic.Zip.dll"
	#packer = ILRepack.new :out => output, :lib => dir
	#packer.merge :lib => dir, :refs => [assembly, 'Ionic.Zip.dll']
end