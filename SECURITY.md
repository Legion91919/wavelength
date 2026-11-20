# Security policy

## Reporting a vulnerability
Please **do not** open a public issue, pull request or discussion for a security problem.

Report it privately instead:
- Use GitHub's **Report a vulnerability** button on this repository's **Security** tab (private vulnerability reporting), or
- Email the security contact listed in the repository profile, with "SECURITY" in the subject.

Include what you found, where (file, endpoint or version), how to reproduce it, and the impact you expect. Please don't access or change data that isn't yours, and give us a chance to fix the problem before you tell anyone else.

## Supported versions
| Version | Supported |
|---|---|
| Latest release (`main`) | Yes |
| Previous minor release | Security fixes only, for 3 months after the next release |
| Anything older | No |

## What happens next
| Step | Target time |
|---|---|
| We confirm we received your report | 2 working days |
| We tell you whether we can reproduce it and how severe we think it is | 5 working days |
| Fix released for Critical / High | 14 days |
| Fix released for Medium / Low | 90 days |

We keep you updated at least weekly until it is fixed, credit you in the release notes if you want, and publish a security advisory once a fix is available.

## Scope
In scope: the code in this repository and the services it deploys. Out of scope: denial-of-service by flooding, social engineering, and issues in third-party services (report those to their owners).
