# Day 1 Student Setup Checklist

## Pre-Session Setup Requirements

### Essential Tools (Pre-flight Check):

#### ✅ Postman Desktop (Required)
- [ ] **Download & Install**: [https://www.postman.com/downloads/](https://www.postman.com/downloads/)
- [ ] **Important**: Use Desktop version, not web version
- [ ] **Test**: Can create and send a GET request
- [ ] **Verify**: Can import collections from JSON

**Why Postman**: We'll be exploring 5 different APIs and building collections for comparison.

#### ✅ VS Code with REST Client Extension
- [ ] **Download VS Code**: [https://code.visualstudio.com/](https://code.visualstudio.com/)
- [ ] **Install REST Client**: 
  1. Open VS Code
  2. Go to Extensions (Ctrl+Shift+X)
  3. Search: "REST Client" by Huachao Mao
  4. Install
- [ ] **Test**: Create a `.http` file and make a simple request

**Why VS Code + REST Client**: Alternative to Postman, great for version-controlled API testing.

#### ✅ Web Browser Setup
- [ ] **Modern browser**: Chrome, Firefox, or Edge
- [ ] **Bookmark bar enabled**: We'll save important API documentation links
- [ ] **JSON Formatter extension** (Optional but helpful):
  - Chrome: "JSON Formatter"
  - Firefox: "JSONView"

#### ✅ Excalidraw Access (For Domain Modeling)
- [ ] **Test access**: [https://excalidraw.com](https://excalidraw.com)
- [ ] **Can create shapes**: Practice drawing boxes and connections
- [ ] **Can share board**: Try sharing a link with someone

**Alternative**: If Excalidraw doesn't work, we have backup options (Google Drawings, physical materials).

#### ✅ GitHub Account
- [ ] **Have account**: [https://github.com](https://github.com) (create if needed)
- [ ] **Can access**: Logged in and working
- [ ] **Know your username**: You'll need it for repository access

---

## Optional Tools (Nice to Have):

#### Git Client
- [ ] **GitHub Desktop**: [https://desktop.github.com](https://desktop.github.com) (recommended for beginners)
- [ ] **OR Command line git**: Test with `git --version`

#### Physical Materials (If Available):
- [ ] **Sticky notes**: 3 different colors (yellow, blue, pink)
- [ ] **Markers**: For writing on sticky notes
- [ ] **Large paper/whiteboard**: For domain modeling

---

## Pre-Session Verification

### Test Your Setup (5 minutes):

#### 1. Postman Test:
```
1. Open Postman Desktop
2. Create new request: GET https://jsonplaceholder.typicode.com/posts/1
3. Send request
4. Verify you get JSON response with a post object
```

#### 2. VS Code REST Client Test:
```
1. Create new file: test.http
2. Add content:
   GET https://jsonplaceholder.typicode.com/posts/1
3. Click "Send Request" link above the line
4. Verify response appears in VS Code
```

#### 3. Excalidraw Test:
```
1. Go to excalidraw.com
2. Draw a rectangle and add text "Test"
3. Click share button and get shareable link
```

---

## Knowledge Prerequisites

### No Prior Experience Required With:
- Domain-Driven Design (we'll teach this)
- OpenAPI/Swagger specification
- gRPC or Protocol Buffers
- Advanced HTTP status codes

### Helpful Background (But Not Required):
- [ ] **Basic HTTP knowledge**: GET, POST, PUT, DELETE
- [ ] **JSON format familiarity**: Can read JSON responses  
- [ ] **API usage experience**: Have used any REST API before
- [ ] **Command line comfort**: Basic terminal/command prompt use

---

## Day 1 Mental Preparation

### Mindset Shift:
We're not building academic examples - we're designing production-ready APIs that real teams would use.

### What to Expect:
- **80% Hands-on**: You'll be actively designing, not passively listening
- **Real-world focus**: Every concept tied to production scenarios
- **Collaborative learning**: Working in pairs and small groups
- **Business thinking**: APIs should speak business language, not database language

### What to Bring:
- [ ] **Worst API experience**: Be ready to share a 2-minute story about the worst API you've had to integrate with
- [ ] **Curiosity**: Questions are encouraged throughout
- [ ] **Production mindset**: Think "How would this work at scale?"

---

## Course Resources (Available Day 1)

### GitHub Repository:
- **URL**: [Will be provided in class]
- **Contains**: All worksheets, templates, examples, and reference materials

### Shared Postman Workspace:
- **URL**: [Will be provided in class]  
- **Contains**: Pre-built API collections for exploration

### Excalidraw Templates:
- **URLs**: [Will be provided in class]
- **Contains**: Domain modeling canvas and API design templates

---

## Troubleshooting Common Issues

### Postman Desktop Won't Install:
**Solution**: Use Postman Web version as backup
**URL**: [https://web.postman.co/](https://web.postman.co/)

### VS Code REST Client Not Working:
**Solution**: Use browser developer tools for GET requests
**How**: Press F12 in browser, use Network tab

### Excalidraw Access Blocked:
**Solution**: Use Google Drawings as alternative
**URL**: [https://docs.google.com/drawings](https://docs.google.com/drawings)

### Internet Connection Issues:
**Solution**: Download offline materials from GitHub repository
**Note**: We'll provide printed worksheets as backup

---

## Contact & Support

### Before Class:
**Questions about setup?** 
- Check this document first
- Try the alternative tools mentioned
- Contact instructor with specific error messages

### During Class:
**Technical issues?**
- Raise your hand immediately
- We have backup plans for every tool
- Focus on learning, not perfect tool setup

---

## Final Checklist

Before Day 1 starts, verify:
- [ ] Postman Desktop working (or web version as backup)
- [ ] VS Code with REST Client extension installed
- [ ] Can access Excalidraw (or Google Drawings backup)
- [ ] GitHub account accessible
- [ ] Stable internet connection
- [ ] Ready to share worst API experience story
- [ ] Excited to build production-ready APIs!

---

**Remember**: Perfect tool setup is nice, but not required. We have backup plans for everything. Focus on learning and participating!

**See you on Day 1!** 🚀 