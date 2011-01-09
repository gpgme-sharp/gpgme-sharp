# gpgme-sharp - .NET binding for libgpgme (GnuPG Made Easy)
# Copyright (C) 2009 Daniel Mueller <daniel@danm.de>
#
# This library is free software; you can redistribute it and/or
# modify it under the terms of the GNU Lesser General Public
# License as published by the Free Software Foundation; either
# version 2.1 of the License, or (at your option) any later version.
#
# This library is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
# Lesser General Public License for more details.
#
# You should have received a copy of the GNU Lesser General Public
# License along with this library; if not, write to the Free Software
# Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, 
# MA  02110-1301  US
#

# development tools 
MONODOCER=monodocer
MONODOC=monodoc
MONODOCS2HTML=monodocs2html
MDASSEMBLER=mdassembler

NAME=GPGME
BASEDIR=../gpgme-sharp
HTMLDIR=htmldocs
DOCLANG=en
CONFTYPE=Release

LIBNAME=gpgme-sharp.dll

BUILDDIR=$(BASEDIR)/bin/$(CONFTYPE)

doc: $(BUILDDIR)/$(LIBNAME)
	mkdir -p $(DOCLANG)/$(NAMESPACE)
	$(MONODOCER) -assembly:$(BUILDDIR)/$(LIBNAME) \
		-path:$(DOCLANG) \
		-pretty \
		-name:$(NAME) 

edit: 
	$(MONODOC) --edit $(DOCLANG)

$(DOCLANG)/index.xml: doc

htmldocs: $(DOCLANG)/index.xml
	 $(MONODOCS2HTML) --source $(DOCLANG) \
	 	--dest $(HTMLDIR)

