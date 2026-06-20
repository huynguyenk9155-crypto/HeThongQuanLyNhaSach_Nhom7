// Chatbot Widget Script
document.addEventListener('DOMContentLoaded', function() {
    const chatbotToggle = document.getElementById('chatbot-toggle');
    const chatbotBox = document.getElementById('chatbot-box');
    const chatbotMinimize = document.getElementById('chatbot-minimize');
    const chatbotInput = document.getElementById('chatbot-input');
    const chatbotSend = document.getElementById('chatbot-send');
    const chatbotMessages = document.getElementById('chatbot-messages');
    const chatbotLoading = document.getElementById('chatbot-loading');
    const notificationCount = document.getElementById('notification-count');

    let isOpen = false;
    let conversationHistory = [];

    // Toggle chatbot box
    chatbotToggle.addEventListener('click', function() {
        isOpen ? closeChatbot() : openChatbot();
    });

    // Minimize chatbot
    chatbotMinimize.addEventListener('click', function(e) {
        e.stopPropagation();
        closeChatbot();
    });

    // Send message on button click
    chatbotSend.addEventListener('click', sendMessage);

    // Send message on Enter key
    chatbotInput.addEventListener('keypress', function(e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });

    function openChatbot() {
        isOpen = true;
        chatbotBox.style.display = 'flex';
        chatbotToggle.style.display = 'none';
        chatbotInput.focus();
        notificationCount.style.display = 'none';
    }

    function closeChatbot() {
        isOpen = false;
        chatbotBox.style.display = 'none';
        chatbotToggle.style.display = 'flex';
    }

    function sendMessage() {
        const message = chatbotInput.value.trim();
        if (!message) return;

        // Add user message to UI
        addMessageToUI(message, 'user');
        conversationHistory.push({ role: 'user', content: message });

        // Clear input
        chatbotInput.value = '';
        chatbotSend.disabled = true;

        // Show loading
        showLoading(true);

        // Send to API
        fetch('/api/chatbot/send-message', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                message: message,
                context: conversationHistory.map(m => `${m.role}: ${m.content}`).join('\n')
            })
        })
        .then(response => response.json())
        .then(data => {
            showLoading(false);
            chatbotSend.disabled = false;

            if (data.success) {
                let botMessage = data.message;
                const actionRegex = /\[ACTION:\s*ADD_TO_CART,\s*(\{[\s\S]*\})\s*\]/;
                const actionMatch = botMessage.match(actionRegex);
                let actionData = null;

                if (actionMatch) {
                    try {
                        actionData = JSON.parse(actionMatch[1].trim());
                        // Remove the action string from the visible response
                        botMessage = botMessage.replace(actionMatch[0], '').trim();
                    } catch (e) {
                        console.error('Failed to parse action JSON:', e);
                    }
                }

                // Add bot response to UI
                addMessageToUI(botMessage, 'bot');
                conversationHistory.push({ role: 'bot', content: botMessage });

                // If there's an action, process it
                if (actionData && actionData.items && actionData.items.length > 0) {
                    processCartAction(actionData.items);
                }
            } else {
                addMessageToUI('Xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại.', 'bot');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showLoading(false);
            chatbotSend.disabled = false;
            addMessageToUI('Xin lỗi, không thể kết nối. Vui lòng thử lại.', 'bot');
        });
    }

    function processCartAction(items) {
        // Show a helper status in chatbot
        addMessageToUI('🤖 Trợ lý AI đang thêm sản phẩm vào giỏ hàng...', 'system');
        
        let promises = items.map(item => {
            let params = new URLSearchParams();
            params.append('bookId', item.id);
            params.append('quantity', item.quantity);

            return fetch('/Cart/AddToCartJson', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: params
            })
            .then(res => res.json());
        });

        Promise.all(promises)
        .then(results => {
            // Remove the system helper message
            const systemMsgs = document.querySelectorAll('.system-message');
            if (systemMsgs.length > 0) {
                systemMsgs[systemMsgs.length - 1].remove();
            }

            let successCount = 0;
            let lastCartCount = 0;
            let messages = [];

            results.forEach((res, index) => {
                if (res.success) {
                    successCount++;
                    lastCartCount = res.cartCount;
                } else {
                    messages.push(`Lỗi thêm "${items[index].title}": ${res.message}`);
                }
            });

            if (successCount > 0) {
                // Update header cart badge
                const cartBadge = document.getElementById('cartBadge');
                if (cartBadge) {
                    cartBadge.textContent = lastCartCount;
                    cartBadge.style.display = 'inline-block';
                }

                // Append buttons for checkout
                const botMessageDivs = document.querySelectorAll('.bot-message');
                const lastBotMsgDiv = botMessageDivs[botMessageDivs.length - 1];
                if (lastBotMsgDiv) {
                    const contentDiv = lastBotMsgDiv.querySelector('.message-content');
                    if (contentDiv) {
                        const buttonsContainer = document.createElement('div');
                        buttonsContainer.style.marginTop = '10px';
                        buttonsContainer.style.display = 'flex';
                        buttonsContainer.style.gap = '8px';
                        buttonsContainer.innerHTML = `
                            <a href="/Cart" style="background:#5e72e4; color:white; padding:6px 12px; border-radius:4px; font-size:12px; text-decoration:none; display:inline-block; font-weight:bold;">🛒 Xem giỏ hàng</a>
                            <a href="/Cart/Checkout" style="background:#2dce89; color:white; padding:6px 12px; border-radius:4px; font-size:12px; text-decoration:none; display:inline-block; font-weight:bold;">💳 Thanh toán ngay</a>
                        `;
                        contentDiv.appendChild(buttonsContainer);
                    }
                }
                
                // Show notification toast if available
                if (typeof showToast === 'function') {
                    showToast('success', `Đã thêm ${successCount} sản phẩm vào giỏ hàng bằng trợ lý AI!`);
                }
            }

            if (messages.length > 0) {
                addMessageToUI('⚠️ ' + messages.join('<br>'), 'bot');
            }
        })
        .catch(err => {
            console.error('Error processing chatbot action:', err);
            addMessageToUI('❌ Không thể tự động thêm vào giỏ hàng. Vui lòng thử lại thủ công.', 'bot');
        });
    }

    function addMessageToUI(message, sender) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${sender}-message`;

        const contentDiv = document.createElement('div');
        contentDiv.className = 'message-content';
        
        // Parse markdown-like formatting
        let formattedMessage = message
            .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
            .replace(/\*(.*?)\*/g, '<em>$1</em>')
            .replace(/\n/g, '<br>');
        
        contentDiv.innerHTML = formattedMessage;

        const timeDiv = document.createElement('div');
        timeDiv.className = 'message-time';
        const now = new Date();
        timeDiv.textContent = now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });

        messageDiv.appendChild(contentDiv);
        messageDiv.appendChild(timeDiv);

        if (sender === 'system') {
            messageDiv.style.alignSelf = 'center';
            messageDiv.style.backgroundColor = '#f8f9fe';
            messageDiv.style.color = '#8898aa';
            messageDiv.style.fontSize = '12px';
            messageDiv.style.borderRadius = '20px';
            messageDiv.style.padding = '4px 12px';
            messageDiv.style.margin = '4px auto';
            messageDiv.style.border = '1px dashed #e9ecef';
            timeDiv.style.display = 'none';
        }

        chatbotMessages.appendChild(messageDiv);

        // Auto scroll to bottom
        chatbotMessages.scrollTop = chatbotMessages.scrollHeight;
    }

    function showLoading(show) {
        chatbotLoading.style.display = show ? 'flex' : 'none';
        
        if (show) {
            const loadingDiv = document.createElement('div');
            loadingDiv.className = 'message bot-message';
            loadingDiv.id = 'loading-message';
            
            const contentDiv = document.createElement('div');
            contentDiv.className = 'message-content';
            contentDiv.innerHTML = '<div class="spinner"></div>';
            
            loadingDiv.appendChild(contentDiv);
            chatbotMessages.appendChild(loadingDiv);
            chatbotMessages.scrollTop = chatbotMessages.scrollHeight;
        } else {
            const loadingMessage = document.getElementById('loading-message');
            if (loadingMessage) {
                loadingMessage.remove();
            }
        }
    }

    // Keyboard shortcuts
    document.addEventListener('keydown', function(e) {
        // Ctrl+Shift+M to toggle chatbot
        if (e.ctrlKey && e.shiftKey && e.key === 'M') {
            e.preventDefault();
            isOpen ? closeChatbot() : openChatbot();
        }
    });

    // Optional: Show notification when new message arrives (if not focused)
    window.addEventListener('blur', function() {
        if (isOpen) {
            // Could add unread indicator here
        }
    });
});
