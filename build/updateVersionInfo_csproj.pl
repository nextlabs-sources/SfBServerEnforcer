#!/usr/bin/perl
#
# DESCRIPTION
# This script updates VERSIONINFO section in an existing Visual Studio project resouce file
# using version information in Makefile.inc. It should be called from Pre-Build Event of
# a VS project.

use strict;
use warnings;

print "NextLabs Update Resource File VersionInfo Script (.csproj Pre-Build Event)\n";


#
# Check for errors
#

# -----------------------------------------------------------------------------
# Dump parameters

sub dumpParameters
{
	print "ERROR: Wrong # of arguments (expect 5).\n";
	print "  See updateVersionInfo_vsproj.pl and Makefile in //depot/dev/D_SiriusR2/build/ for more details.\n";
	print "Argv[0] = $ARGV[0]\n";
	print "Argv[1] = $ARGV[1]\n";
	print "Argv[2] = $ARGV[2]\n";
	print "Argv[3] = $ARGV[3]\n";
	print "Argv[4] = $ARGV[4]\n";
}

my	$argCount = scalar(@ARGV);

dumpParameters;

#
# Process parameters
#

# -----------------------------------------------------------------------------
# Print usage

sub printUsage
{
	print "usage: updateVersionInfo_vsproj.pl <resource file> <build code>\n";
	print "  build code    - Build code. Valid values are number, 'dev' and 'nightly'.\n";
	print "                  Specify a number for release build.\n";
	print "  resource file - Resource file containing VERSIONINFO section to be updated.\n";
	print "                  The path is relative to location of .vsproj file.\n";
}

if (($argCount != 5) || $ARGV[0] eq "-h" || $ARGV[0] eq "--help")
{
	printUsage;
	exit 1;
}

# Collect parameters
my	$resourceFile = $ARGV[0];
my	$product = $ARGV[1];
my	$majorVer = $ARGV[2];
my	$minorVer = $ARGV[3];
my	$build = $ARGV[4];
my	$showUpdatedFile = 0;

# Print parameters
print "Parameters:\n";
print "  Resource File       = $resourceFile\n";
print "  Show Updated File   = $showUpdatedFile\n";


#
# Check for errors
#

if ( ! -e $resourceFile)
{
	print "ERROR: $resourceFile does not exist\n";
	exit 1;
}

print "Makefile Version Data:\n";
print "  Product Name        = $product\n";
print "  Major Version       = $majorVer\n";
print "  Minor Version       = $minorVer\n";

#
# Read resource file
#

local $/ = undef;
open FILE, $resourceFile || die "Error opening resource file $resourceFile (read)";
my	$buf = <FILE>;
close FILE;

#print "\nSource Data:\n----------------\n$buf\n\n";


#
# Update version info
#

$product=~ s/\s*\n//g;
$product=~ s/\s*\r//g;
$buf =~ s/\[assembly:\s+AssemblyVersion\("\s*[^"]*"\)\]/\[assembly: AssemblyVersion(\"$majorVer.$minorVer.$build.0\")\]/g;
$buf =~ s/\[assembly:\s+AssemblyCompany\("\s*[^"]*"\)\]/\[assembly: AssemblyCompany(\"NextLabs, Inc.\")\]/g;
$buf =~ s/\[assembly:\s+AssemblyCopyright\("\s*[^"]*"\)\]/\[assembly: AssemblyCopyright(\"Copyright (C) 2016-2017 NextLabs, Inc. All rights reserved.\")\]/g;
$buf =~ s/\[assembly:\s+AssemblyProduct\("\s*[^"]*"\)\]/\[assembly: AssemblyProduct(\"$product\")\]/g;
$buf =~ s/\[assembly:\s+AssemblyFileVersion\("\s*[^"]*"\)\]/\[assembly: AssemblyFileVersion(\"$majorVer.$minorVer.$build.0\")\]/g;

#print "\nUpdated Data:\n----------------\n$buf\n\n";


#
# Write resource file
#
# Notes: There is a problem with Cygwin + Perforce combination. If you run chmod from
# Cygwin, you will get an error. If you run "ls -al" you will see no permission and 
# group is mkpasswd. If you check "if (-r myfile)", it will always return true. To 
# work around this problem, we call Windows ATTRIB command directly.

my	$resourceFileDos = $resourceFile;

$resourceFileDos =~ s#/#\\#;

system("ATTRIB -R \"$resourceFileDos\"");

#if (chmod(0777, $resourceFile) == 0)
#{
#	die "### ERROR: Failed to chmod on file $resourceFile\n";
#}

open FILE, ">$resourceFile" || die "Error opening resource file $resourceFile (write)";
print FILE $buf;
close FILE;


#
# Print updated file
#

if ($showUpdatedFile)
{
	open FILE, $resourceFile || die "Error opening updated file $resourceFile\n";

	while (<FILE>)
	{
		print $_;
	}

	close FILE;
}

exit 0;
