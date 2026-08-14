<?php

/*
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
 WAMP-DS phpMyAdmin Configuration
 phpMyAdmin 5.2.x - Windows Development Environment

 Local development configuration for WAMP-DS.
 NOT intended for production servers.

 This file is generated and controlled by WAMP-DS.
 Manual changes may be overwritten.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
*/

declare(strict_types=1);


/**
 * WAMP-DS generated secret used by phpMyAdmin cookie authentication.
 *
 * WAMP-DS replaces this value during installation with a
 * cryptographically secure random secret.
 */
$cfg['blowfish_secret'] = '{{BLOWFISH_SECRET}}';


/**
 * MySQL server configuration.
 */

$i = 0;

$i++;

$cfg['Servers'][$i]['auth_type'] = 'cookie';

$cfg['Servers'][$i]['host'] = '127.0.0.1';

$cfg['Servers'][$i]['port'] = 3306;

$cfg['Servers'][$i]['compress'] = false;

$cfg['Servers'][$i]['AllowNoPassword'] = true;


/**
 * phpMyAdmin configuration storage.
 *
 * WAMP-DS does not enable phpMyAdmin configuration storage by default.
 *
 * These settings can be enabled in the future if WAMP-DS creates and
 * manages the phpMyAdmin configuration database.
 */


/**
 * Directories for saving/loading files from the server.
 */

$cfg['UploadDir'] = '';

$cfg['SaveDir'] = '';


/**
 * WAMP-DS development defaults.
 *
 * phpMyAdmin uses its standard defaults for all other configuration
 * options unless explicitly overridden by WAMP-DS.
 *
 * WAMP-DS should not modify phpMyAdmin's application files.
 */


/**
 * End of WAMP-DS configuration.
 */